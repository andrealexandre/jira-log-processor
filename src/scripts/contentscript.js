import ext from "./utils/ext";
import $ from "jquery";

var extractTags = () => {
  var url = document.location.href;
  if(!url || !url.match(/^http/)) return;

  var data = {
    title: "",
    description: "",
    url: document.location.href
  }

  var ogTitle = document.querySelector("meta[property='og:title']");
  if(ogTitle) {
    data.title = ogTitle.getAttribute("content")
  } else {
    data.title = document.title
  }

  var descriptionTag = document.querySelector("meta[property='og:description']") || document.querySelector("meta[name='description']")
  if(descriptionTag) {
    data.description = descriptionTag.getAttribute("content")
  }

  return data;
}

function onRequest(request, sender, sendResponse) {
  console.log(request.action)
  if (request.action === 'process-page') {
    sendResponse(extractTags())
  }
}

function onStartup() { 
  detectJiraCsvExport()
}

function detectJiraCsvExport() {
  let jiraIssueHeader = $('.aui-page-header-main')
  if(jiraIssueHeader.length) {
    console.log('JIRA issue header exists')
    // jiraIssueHeader.css('background-color', 'red')
    findCsvExport()
  }
  else {
    console.warn('JIRA issue header DOESN\'T exists')
  }
}

function findCsvExport() {
  let csvAnchor = $('#jira\\.issueviews\\:issue-csv')
  if(csvAnchor.length) {
    console.log("Found anchor for JIRA CSV export")
    console.log(csvAnchor)
  }
  else {
    console.log("No CSV export found for this JIRA")
  }
}

ext.runtime.onMessage.addListener(onRequest);
window.addEventListener('load', onStartup);