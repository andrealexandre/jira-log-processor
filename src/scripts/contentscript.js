import ext from "./utils/ext";

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
  console.log('onStartup')
  let jiraIssueHeader = document.getElementById('jira-issue-header')
  if(jiraIssueHeader) {
    jiraIssueHeader.style.backgroundColor = 'red'
  }
}

ext.runtime.onMessage.addListener(onRequest);
ext.runtime.onStartup.addListener(onStartup);