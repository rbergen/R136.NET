﻿var R136JS = R136JS || {};

R136JS.scrollToElementId = function(elementId) {
  var element = document.getElementById(elementId);

  if (!element)
      return false;

  element.scrollIntoView({ behavior: "smooth" });

  return true;
}

R136JS.stretchToHeight = function(from, to) {
  var fromElement = $("#" + from);
  var toElement = $("#" + to);

  if (fromElement == null || toElement == null)
    return false;

  toElement.height(fromElement.outerHeight());

  return true;
}

R136JS.setClipboard = function(text) {
  navigator.clipboard.writeText(text);
}

R136JS.enableTooltips = function () {
  R136JS.tooltipList = Array.from(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map((element) => new bootstrap.Tooltip(element));
}

R136JS.closeTooltips = function () {
  if (!R136JS.tooltipList)
    return;

  R136JS.tooltipList.map((tooltip) => tooltip.hide());
}

R136JS.blinkTexts = [];
R136JS.blinkCount = 0;

R136JS.blinkInit = function() {
  if ($("#blinkTextMessage")) {
    $(".blinkTextContent").each(function() {
      R136JS.blinkTexts[R136JS.blinkCount++] = $(this).text();
    });

    return true;
  }

  return false;
}

R136JS.blinkText = function() {
  var blinkTextDiv = $("#blinkTextMessage");
  if (!blinkTextDiv)
    return;

  if (this.blinkCount >= this.blinkTexts.length)
    this.blinkCount = 0;

  blinkTextDiv.html(this.blinkTexts[this.blinkCount++]);
  blinkTextDiv.fadeIn(300).animate({ opacity: 1.0 }).fadeOut(300, () => this.blinkText());
}

if (R136JS.blinkInit())
  R136JS.blinkText();