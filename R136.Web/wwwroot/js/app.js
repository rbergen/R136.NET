window.scrollToElementId = (elementId) => {
  var element = document.getElementById(elementId);
  if(!element)
      return false;

  element.scrollIntoView({ behavior: "smooth" });
  return true;
}

window.stretchToHeight = (from, to) => {
  var fromElement = $("#" + from);
  var toElement = $("#" + to);
  if (fromElement == null || toElement == null)
    return false;
  if (fromElement.height() > toElement.height())
    toElement.height(fromElement.height());
  return true;
}

