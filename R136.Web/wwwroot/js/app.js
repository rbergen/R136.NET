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
//  if (fromElement.height() > toElement.height())
    toElement.height(fromElement.height());
  return true;
}

var blinkTexts = [];
var blinkCount = 0;

if ($("#blinkTextMessage")) {
  $(".blinkTextContent").each(function () {
    blinkTexts[blinkCount++] = $(this).text();
  });
}

function blinkText() {
  var blinkTextDiv = $("#blinkTextMessage");
  if (!blinkTextDiv)
    return;

  if (blinkCount >= blinkTexts.length)
    blinkCount = 0;

  blinkTextDiv.html(blinkTexts[blinkCount++]);
  blinkTextDiv.fadeIn(300).animate({ opacity: 1.0 }).fadeOut(300,
      function () {
        return blinkText();
      }
    );
}
blinkText();
