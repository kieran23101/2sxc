﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav;
using ToSic.SexyContent.DataImportExport.Extensions;

namespace ToSic.SexyContent.DataImportExport
{
    public class DataXmlExport
    {
        /// <summary>
        /// Create a blank xml scheme for 2SexyContent data.
        /// </summary>
        public string SerializeBlank(int? applicationId, int contentTypeId)
        {
            var contentType = GetContentType(applicationId, contentTypeId);
            if (contentType == null) 
                return null;

            var documentElement = GetDocumentEntityElement("", "");
            var documentRoot = GetDocumentRoot(contentType.Name, documentElement);
            var document = GetDocument(documentRoot);

            var attributes = contentType.GetAttributes();
            foreach (var attribute in attributes)
            {
                  documentElement.Append(attribute.StaticName, "");      
            }
       
            return document.ToString();
        }

        /// <summary>
        /// Serialize 2SexyContent data to an xml string.
        /// </summary>
        /// <param name="applicationId">ID of 2SexyContent application</param>
        /// <param name="contentTypeId">ID of 2SexyContent type</param>
        /// <param name="languageSelected">Language of the data to be serialized (null for all languages)</param>
        /// <param name="languageFallback">Language fallback of the system</param>
        /// <param name="languageScope">Languages supported of the system</param>
        /// <param name="languageReferenceOption">How value references to other languages are handled</param>
        /// <param name="resourceReferenceOption">How value references to files and pages are handled</param>
        public string Serialize(int? applicationId, int contentTypeId, string languageSelected, string languageFallback, IEnumerable<string> languageScope, LanguageReferenceExportOption languageReferenceOption, ResourceReferenceExportOption resourceReferenceOption)
        {
            var contentType = GetContentType(applicationId, contentTypeId);
            if (contentType == null)
                return null;

            var languages = new List<string>();
            if (!string.IsNullOrEmpty(languageSelected))
            {
                languages.Add(languageSelected);
            }
            else if (languageScope.Any())
            {
                languages.AddRange(languageScope);
            }
            else
            {
                languages.Add(languageFallback);
            }

            var documentRoot = GetDocumentRoot(contentType.Name, null);
            var document = GetDocument(documentRoot);

            var entities = contentType.Entities.Where(entity => entity.ChangeLogIDDeleted == null);
            foreach (var entity in entities)
            {
                foreach (var language in languages)
                {  
                    var documentElement = GetDocumentEntityElement(entity.EntityGUID, language);
                    documentRoot.Add(documentElement);
                    
                    var attributes = contentType.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        if (languageReferenceOption.IsResolve())
                        {
                            documentElement.AppendValueResolved(entity, attribute, language, languageFallback, resourceReferenceOption);
                        }
                        else
                        {
                            documentElement.AppendValueReferenced(entity, attribute, language, languageFallback, languageScope, languages.Count() > 1, resourceReferenceOption);
                        }
                    }
                }
            }

            return document.ToString();
        }


        private static AttributeSet GetContentType(int? applicationId, int contentTypeId)
        {
            var contentContext = new SexyContent(true, new int?(), applicationId).ContentContext;
            return contentContext.GetAttributeSet(contentTypeId);
        }

        private static XDocument GetDocument(params object[] content)
        {
            return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), content);
        }

        private static XElement GetDocumentRoot(string contentTypeName, params object[] content)
        {
            return new XElement(XElementName.Root + contentTypeName.RemoveSpecialCharacters(), content);
        }

        private static XElement GetDocumentEntityElement(object elementGuid, object elementLanguage)
        {
            return new XElement
                (
                    XElementName.Entity, 
                    new XElement(XElementName.EntityGuid, elementGuid), 
                    new XElement(XElementName.EntityLanguage, elementLanguage)
                );
        }
    }
}