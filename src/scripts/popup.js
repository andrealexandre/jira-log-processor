import ext from "./utils/ext";
import storage from "./utils/storage";
import $ from "jquery";
import Papa from "papaparse";

var popup = document.getElementById("app");
storage.get('color', function(resp) {
  var color = resp.color;
  if(color) {
    popup.style.backgroundColor = color
  }
});

var template = (data) => {
  var json = JSON.stringify(data);
  return (`
  <div class="site-description">
    <h3 class="title">${data.title}</h3>
    <p class="description">${data.description}</p>
    <a href="${data.url}" target="_blank" class="url">${data.url}</a>
  </div>
  <div class="action-container">
    <button data-bookmark='${json}' id="save-btn" class="btn btn-primary">Save</button>
  </div>
  `);
}
var renderMessage = (message) => {
  var displayContainer = $("#display-container");
  displayContainer.html(`<p class='message'>${message}</p>`);
}

var renderBookmark = (data) => {
  var displayContainer = $("#display-container")
  if(data) {
    var tmpl = template(data);
    displayContainer.html(tmpl);
  } else {
    renderMessage("Sorry, could not extract this page's title and URL")
  }
}

ext.tabs.query({active: true, currentWindow: true}, function(tabs) {
  var activeTab = tabs[0];
  chrome.tabs.sendMessage(activeTab.id, { action: 'process-page' }, renderBookmark);
});

popup.addEventListener("click", function(e) {
  if(e.target && e.target.matches("#save-btn")) {
    e.preventDefault();
    var data = e.target.getAttribute("data-bookmark");
    ext.runtime.sendMessage({ action: "perform-save", data: data }, function(response) {
      if(response && response.action === "saved") {
        renderMessage("Your bookmark was saved successfully!");
      } else {
        renderMessage("Sorry, there was an error while saving your bookmark.");
      }
    })
  }
});

var optionsLink = document.querySelector(".js-options");
optionsLink.addEventListener("click", function(e) {
  e.preventDefault();
  ext.tabs.create({'url': ext.extension.getURL('options.html')});
})

function devLogAppend(message) {
  const devLog = $('#dev-log')
  devLog.append(`<p>${message}</p>`)
}

function logEvent(event) {
  console.log(`${typeof event.target}.${event.type}:`)
  console.log(event)
}

$('#csv-form').on('submit', (event) => {
  event.preventDefault()
  let file = event.target[0].files[0];

  Papa.parse(file, {
    complete: (results) => {
      results.data.forEach(elem => devLogAppend(elem))
    }
  })

});