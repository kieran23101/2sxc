!function(t){var e={};function n(r){if(e[r])return e[r].exports;var i=e[r]={i:r,l:!1,exports:{}};return t[r].call(i.exports,i,i.exports,n),i.l=!0,i.exports}n.m=t,n.c=e,n.d=function(t,e,r){n.o(t,e)||Object.defineProperty(t,e,{enumerable:!0,get:r})},n.r=function(t){"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(t,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(t,"__esModule",{value:!0})},n.t=function(t,e){if(1&e&&(t=n(t)),8&e)return t;if(4&e&&"object"==typeof t&&t&&t.__esModule)return t;var r=Object.create(null);if(n.r(r),Object.defineProperty(r,"default",{enumerable:!0,value:t}),2&e&&"string"!=typeof t)for(var i in t)n.d(r,i,function(e){return t[e]}.bind(null,i));return r},n.n=function(t){var e=t&&t.__esModule?function(){return t.default}:function(){return t};return n.d(e,"a",e),e},n.o=function(t,e){return Object.prototype.hasOwnProperty.call(t,e)},n.p="",n(n.s=3)}([function(t,e,n){"use strict";var r,i=this&&this.__extends||(r=function(t,e){return(r=Object.setPrototypeOf||{__proto__:[]}instanceof Array&&function(t,e){t.__proto__=e}||function(t,e){for(var n in e)e.hasOwnProperty(n)&&(t[n]=e[n])})(t,e)},function(t,e){function n(){this.constructor=t}r(t,e),t.prototype=null===e?Object.create(e):(n.prototype=e.prototype,new n)});Object.defineProperty(e,"__esModule",{value:!0});var o=function(t){function e(){return null!==t&&t.apply(this,arguments)||this}return i(e,t),e}(HTMLElement);e.EavCustomInputField=o;var a=function(t){function e(){return null!==t&&t.apply(this,arguments)||this}return i(e,t),e}(o);e.EavCustomInputFieldObservable=a},function(t,e,n){"use strict";Object.defineProperty(e,"__esModule",{value:!0}),e.buildTemplate=function(t,e){return t+"<style>\n"+e+"\n</style>"},e.parseLatLng=function(t){return JSON.parse(t.replace("latitude","lat").replace("longitude","lng"))},e.stringifyLatLng=function(t){return JSON.stringify(t).replace("lat","latitude").replace("lng","longitude")}},function(t,e,n){"use strict";Object.defineProperty(e,"__esModule",{value:!0}),e.defaultCoordinates={lat:47.17465989999999,lng:9.469142499999975};e.mapsParameters={mapApiUrl:"https://maps.googleapis.com/maps/api/js?key=AIzaSyDPhnNKpEg8FmY8nooE7Zwnue6SusxEnHE"}},function(t,e,n){n(4),t.exports=n(9)},function(t,e,n){"use strict";var r,i=this&&this.__extends||(r=function(t,e){return(r=Object.setPrototypeOf||{__proto__:[]}instanceof Array&&function(t,e){t.__proto__=e}||function(t,e){for(var n in e)e.hasOwnProperty(n)&&(t[n]=e[n])})(t,e)},function(t,e){function n(){this.constructor=t}r(t,e),t.prototype=null===e?Object.create(e):(n.prototype=e.prototype,new n)});Object.defineProperty(e,"__esModule",{value:!0});var o=n(5),a=n(1),s=n(2),l=n(6),c=n(7),p=n(8),u=function(t){function e(){var e=t.call(this)||this;return e.mapApiUrl=s.mapsParameters.mapApiUrl,console.log("FieldCustomGps constructor called"),e.fieldInitialized=!1,e.eventListeners=[],e}return i(e,t),e.prototype.connectedCallback=function(){if(console.log("FieldCustomGps connectedCallback called"),!this.fieldInitialized){this.fieldInitialized=!0,this.innerHTML=a.buildTemplate(l,c),this.latInput=this.querySelector("#lat"),this.lngInput=this.querySelector("#lng");var t=this.querySelector("#address-mask-container");this.iconSearch=this.querySelector("#icon-search");var e=this.querySelector("#formatted-address-container");this.mapContainer=this.querySelector("#map");var n=this.experimental.allInputTypeNames.map(function(t){return t.name});-1!==n.indexOf(this.connector.field.settings.LatField)&&(this.latFieldName=this.connector.field.settings.LatField),-1!==n.indexOf(this.connector.field.settings.LongField)&&(this.lngFieldName=this.connector.field.settings.LongField);var r=this.connector.field.settings.AddressMask||this.connector.field.settings["Address Mask"];if(this.addressMaskService=new p.FieldMaskService(r,this.experimental.formGroup.controls,null,null),console.log("FieldCustomGps addressMask:",r),r&&(t.classList.remove("hidden"),e.innerText=this.addressMaskService.resolve()),!!window.google)this.mapScriptLoaded();else{var i=document.createElement("script");i.src=this.mapApiUrl,i.onload=this.mapScriptLoaded.bind(this),this.appendChild(i)}}},e.prototype.mapScriptLoaded=function(){console.log("FieldCustomGps mapScriptLoaded called"),this.map=new google.maps.Map(this.mapContainer,{zoom:15,center:s.defaultCoordinates}),this.marker=new google.maps.Marker({position:s.defaultCoordinates,map:this.map,draggable:!0}),this.geocoder=new google.maps.Geocoder,this.connector.data.value?this.updateHtml(a.parseLatLng(this.connector.data.value)):this.updateHtml(s.defaultCoordinates);var t=this.onLatLngInputChange.bind(this);this.latInput.addEventListener("change",t),this.lngInput.addEventListener("change",t);var e=this.autoSelect.bind(this);this.iconSearch.addEventListener("click",e),this.eventListeners.push({element:this.latInput,type:"change",listener:t},{element:this.lngInput,type:"change",listener:t},{element:this.iconSearch,type:"click",listener:e}),this.marker.addListener("dragend",this.onMarkerDragend.bind(this))},e.prototype.updateHtml=function(t){this.latInput.value=t.lat?t.lat.toString():"",this.lngInput.value=t.lng?t.lng.toString():"",this.map.setCenter(t),this.marker.setPosition(t)},e.prototype.updateForm=function(t){this.connector.data.update(a.stringifyLatLng(t)),this.latFieldName&&this.experimental.updateField(this.latFieldName,t.lat),this.lngFieldName&&this.experimental.updateField(this.lngFieldName,t.lng)},e.prototype.onLatLngInputChange=function(){console.log("FieldCustomGps input changed");var t={lat:this.latInput.value.length>0?parseFloat(this.latInput.value):null,lng:this.lngInput.value.length>0?parseFloat(this.lngInput.value):null};this.updateHtml(t),this.updateForm(t)},e.prototype.autoSelect=function(){var t=this;console.log("FieldCustomGps geocoder called");var e=this.addressMaskService.resolve();this.geocoder.geocode({address:e},function(n,r){if(r===google.maps.GeocoderStatus.OK){var i=n[0].geometry.location,o={lat:i.lat(),lng:i.lng()};t.updateHtml(o),t.updateForm(o)}else alert("Could not locate address: "+e)})},e.prototype.onMarkerDragend=function(t){console.log("FieldCustomGps marker changed");var e={lat:t.latLng.lat(),lng:t.latLng.lng()};this.updateHtml(e),this.updateForm(e)},e.prototype.disconnectedCallback=function(){console.log("FieldCustomGps disconnectedCallback called"),window.google&&(google.maps.event.clearInstanceListeners(this.marker),google.maps.event.clearInstanceListeners(this.map)),this.eventListeners.forEach(function(t){var e=t.element,n=t.type,r=t.listener;e.removeEventListener(n,r)})},e}(o.EavExperimentalInputField);customElements.define("field-custom-gps",u)},function(t,e,n){"use strict";var r,i=this&&this.__extends||(r=function(t,e){return(r=Object.setPrototypeOf||{__proto__:[]}instanceof Array&&function(t,e){t.__proto__=e}||function(t,e){for(var n in e)e.hasOwnProperty(n)&&(t[n]=e[n])})(t,e)},function(t,e){function n(){this.constructor=t}r(t,e),t.prototype=null===e?Object.create(e):(n.prototype=e.prototype,new n)});Object.defineProperty(e,"__esModule",{value:!0});var o=function(t){function e(){return null!==t&&t.apply(this,arguments)||this}return i(e,t),e}(n(0).EavCustomInputField);e.EavExperimentalInputField=o},function(t,e){t.exports='<div class="custom-gps-container">\r\n  <div class="map-info">\r\n    <div class="input-component">\r\n      <label for="lat">Lat:</label>\r\n      <input id="lat" type="number" step="0.001" />\r\n    </div>\r\n    &nbsp;\r\n    <div class="input-component">\r\n      <label for="lng">Lng:</label>\r\n      <input id="lng" type="number" step="0.001" />\r\n    </div>\r\n  </div>\r\n\r\n  <div id="address-mask-container" class="map-info address-mask-container hidden">\r\n    <a id="icon-search" class="btn">\r\n      <i class="material-icons">search</i>\r\n    </a>\r\n    <span id="formatted-address-container"></span>\r\n  </div>\r\n\r\n  <div id="map" class="map-info__map"></div>\r\n</div>\r\n'},function(t,e){t.exports=".custom-gps-container {\r\n  display: flex;\r\n  flex-direction: column;\r\n  height: 100%;\r\n}\r\n\r\n.map-info {\r\n  flex: 0 0 32px;\r\n  display: flex;\r\n  flex-wrap: wrap;\r\n  align-items: center;\r\n  padding: 4px;\r\n  padding-left: 14px;\r\n  border-bottom: 1px solid #e1e1e1;\r\n  background: white;\r\n}\r\n\r\n.address-mask-container {\r\n  flex-wrap: nowrap;\r\n}\r\n\r\n.map-info label,\r\n.map-info #icon-search {\r\n  margin-right: 8px;\r\n  display: flex;\r\n  justify-content: center;\r\n  align-items: center;\r\n  font-size: 12px;\r\n  text-transform: uppercase;\r\n  padding: 4px;\r\n}\r\n\r\n.map-info #icon-search i {\r\n  font-size: 14px;\r\n  width: 14px;\r\n  height: 14px;\r\n  font-weight: bold;\r\n}\r\n\r\n.map-info input {\r\n  margin-right: 8px;\r\n  padding: 4px 16px;\r\n  border: none;\r\n  background: transparent;\r\n  outline: none !important;\r\n}\r\n\r\n.map-info__map {\r\n  flex: 1 1 auto;\r\n  width: 100%;\r\n  display: block;\r\n}\r\n\r\n.hidden {\r\n  display: none;\r\n}\r\n\r\n.btn {\r\n  border: 1px solid silver;\r\n  border-radius: 4px;\r\n}\r\n.btn:hover {\r\n  background-color: rgba(69, 79, 99, 0.08);\r\n  cursor: pointer;\r\n}\r\n\r\n.input-component {\r\n  display: flex;\r\n  background-color: rgba(69, 79, 99, 0.08);\r\n  padding: 4px;\r\n  border-radius: 4px 4px 0 0;\r\n  border-bottom: 1px solid silver;\r\n  margin: 8px 0;\r\n}\r\n.input-component:hover {\r\n  border-bottom: 1px solid #0087f4;\r\n}\r\n"},function(t,e,n){"use strict";Object.defineProperty(e,"__esModule",{value:!0});var r=function(){function t(t,e,n,r){this.changeEvent=n,this.fields=[],this.findFields=/\[.*?\]/gi,this.unwrapField=/[\[\]]/gi,this.subscriptions=[],this.mask=t,this.model=e,this.fields=this.fieldList(),r&&(this.preClean=r),e&&n&&this.watchAllFields()}return t.prototype.resolve=function(){var t=this,e=this.mask;return this.fields.forEach(function(n,r){var i=t.model.hasOwnProperty(n)&&t.model[n]&&t.model[n].value?t.model[n].value:"",o=t.preClean(n,i);e=e.replace("["+n+"]",o)}),e},t.prototype.fieldList=function(){var t=this,e=[];if(!this.mask)return e;var n=this.mask.match(this.findFields);return n?n.forEach(function(n,r){var i=n.replace(t.unwrapField,"");e.push(i)}):e.push(this.mask),e},t.prototype.preClean=function(t,e){return e},t.prototype.onChange=function(){console.log("FieldMaskService onChange called");var t=this.resolve();this.value!==t&&this.changeEvent(t),this.value=t},t.prototype.watchAllFields=function(){var t=this;console.log("FieldMaskService watchAllFields called"),this.fields.forEach(function(e){if(t.model[e]){var n=t.model[e].valueChanges.subscribe(function(e){return t.onChange()});t.subscriptions.push(n)}})},t.prototype.destroy=function(){this.subscriptions.forEach(function(t){return t.unsubscribe()})},t}();e.FieldMaskService=r},function(t,e,n){"use strict";var r,i=this&&this.__extends||(r=function(t,e){return(r=Object.setPrototypeOf||{__proto__:[]}instanceof Array&&function(t,e){t.__proto__=e}||function(t,e){for(var n in e)e.hasOwnProperty(n)&&(t[n]=e[n])})(t,e)},function(t,e){function n(){this.constructor=t}r(t,e),t.prototype=null===e?Object.create(e):(n.prototype=e.prototype,new n)});Object.defineProperty(e,"__esModule",{value:!0});var o=n(0),a=n(1),s=n(2),l=n(10),c=n(11),p=function(t){function e(){var e=t.call(this)||this;return console.log("FieldCustomGpsPreview constructor called"),e}return i(e,t),e.prototype.connectedCallback=function(){var t=this;console.log("FieldCustomGpsPreview connectedCallback called"),this.innerHTML=a.buildTemplate(l,c),this.latContainer=this.querySelector("#lat-container"),this.lngContainer=this.querySelector("#lng-container"),this.connector.data.value?this.updateHtml(a.parseLatLng(this.connector.data.value)):this.updateHtml(s.defaultCoordinates),this.connector.data.onValueChange(function(e){if(e){var n=a.parseLatLng(e);t.updateHtml(n)}else t.updateHtml(s.defaultCoordinates)})},e.prototype.updateHtml=function(t){this.latContainer.innerText=t.lat?t.lat.toString():"",this.lngContainer.innerText=t.lng?t.lng.toString():""},e.prototype.disconnectedCallback=function(){console.log("FieldCustomGpsPreview disconnectedCallback called")},e}(o.EavCustomInputField);customElements.define("field-custom-gps-preview",p)},function(t,e){t.exports='<div class="custom-gps-preview">Lat: <span id="lat-container"></span>, Lng: <span id="lng-container"></span></div>\r\n'},function(t,e){t.exports=".custom-gps-preview {\r\n  position: absolute;\r\n  top: 0;\r\n  bottom: 0;\r\n  display: flex;\r\n  align-items: center;\r\n  white-space: nowrap;\r\n}\r\n"}]);
//# sourceMappingURL=https://sources.2sxc.org/10.04.00/ng-edit/elements/field-custom-gps/gps-picker.js.map