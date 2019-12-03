// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * This method will be called at the start of exports.transform in toc.html.js
 */
exports.preTransform = function (model) {
  // console.warn('2dm-preTransform');
  return model;
}

/**
 * This method will be called at the end of exports.transform in toc.html.js
 */
exports.postTransform = function (model) {
  console.warn('2dm-postTransform:');

  if(isApiToc(model))
    transformItem(model, 1);

  return model;
}

const prefix1 = 'ToSic.Sxc';
const prefix2 = 'ToSic.Eav';

// find out if it's the API toc
function isApiToc(model) {
  if(!model) return false;
  var debugModel = JSON.stringify(model);
  var first100 = debugModel.substr(0, 100);

  // find out if it's the TOC of the API
  if(!(model.items && model.items.length))
    return false;

  var firstName = model.items[0].name;
  var match = isNamespace(firstName);
  // if(!(firstName.indexOf(prefix1) === 0 || firstName.indexOf(prefix2) === 0))
  //   return false;

  if(match)
    console.warn('2dm-addlevel: its the right TOC!' + first100);
  return match;
}

function isNamespace(name) {
  return name && (name.indexOf(prefix1) === 0 || name.indexOf(prefix2) === 0);
}

function repeatString(part, count) {
  if(count <= 0) return "";
  var result = "";
  for(i = 0; i < count; i++)
    result += part;
  return result;
}

const keepParts = 3;

function transformItem(item, level) {
  console.warn('2dm-transformItem');
  // set to null incase mustache looks up
  // item.topicHref = item.topicHref || null;
  // item.tocHref = item.tocHref || null;
  // item.name = item.name || null;

  // <new 2dm
  //const prefix1 = 'ToSic.Sxc.';

  // only do this on namespaces
  if(isNamespace(item.name)) {
    item.fullName = item.name;
    var parts = item.name.split('.');
    var count = parts.length;
    if(count > keepParts) {
      parts.splice(0, count - keepParts);
      var newName = repeatString("...", count - keepParts) + parts.join('.');
      item.name = newName;
    }
  }

  // const replace1 = 'Sxc';
  // console.warn('2dm-name-and-index:' + item.name  + ';idx:' + (item.name && item.name.indexOf(prefix1)));
  // if(item.name && item.name.indexOf(prefix1) == 0) {
  //   console.warn('2dm-replacing "' + item.name + '"');
  //   item.name = item.name.replace(prefix1, replace1);
  //   item._shortname = item.name;
  // }
  // item.XXX = 'YYY';
  // new 2dm>

  item.level = level;
  if (item.items && item.items.length > 0) {
    var length = item.items.length;
    for (var i = 0; i < length; i++) {
      transformItem(item.items[i], level + 1);
    };
  } 
  else {
    // console.warn('2dm-toc-post-TI:' + JSON.stringify(item));
  }
  // else {
  //   item.items = [];
  //   item.leaf = true;
  // }
}