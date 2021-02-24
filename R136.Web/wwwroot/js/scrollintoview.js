window.scrollToElementId = (elementId) => {
  var element = document.getElementById(elementId);
  if(!element)
      return false;

  element.scrollIntoView({ behavior: "smooth" });
  return true;
}
