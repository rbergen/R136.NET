var R136;
(function (R136) {
    var Interop;
    (function (Interop) {
        var Tools = /** @class */ (function () {
            function Tools() {
                this.blinkTexts = [];
                this.blinkCount = 0;
                if (this.blinkInit())
                    this.blinkText();
            }
            Tools.prototype.scrollToElementId = function (elementId) {
                var element = document.getElementById(elementId);
                if (!element)
                    return false;
                element.scrollIntoView({ behavior: "smooth" });
                return true;
            };
            Tools.prototype.adoptVerticalPadding = function (targetId, headerId, footerId) {
                var target = document.getElementById(targetId);
                var header = document.getElementById(headerId);
                var footer = document.getElementById(footerId);
                if (!target)
                    return;
                if (header)
                    target.style.paddingTop = header.offsetHeight + "px";
                if (footer)
                    target.style.paddingBottom = footer.offsetHeight + "px";
            };
            Tools.prototype.setClipboard = function (text) {
                navigator.clipboard.writeText(text);
            };
            Tools.prototype.enableTooltips = function () {
                this.tooltipList = Array.from(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map(function (element) { return new Bootstrap.Tooltip(element); });
            };
            Tools.prototype.closeTooltips = function () {
                if (this.tooltipList)
                    this.tooltipList.forEach(function (tooltip) { return tooltip.hide(); });
            };
            Tools.prototype.blinkInit = function () {
                var _this = this;
                if ($("#blinkTextMessage")) {
                    $(".blinkTextContent").each(function (_index, element) {
                        _this.blinkTexts.push($(element).text());
                    });
                    return true;
                }
                return false;
            };
            Tools.prototype.blinkText = function () {
                var _this = this;
                var blinkTextDiv = $("#blinkTextMessage");
                if (!blinkTextDiv)
                    return;
                if (this.blinkCount >= this.blinkTexts.length)
                    this.blinkCount = 0;
                blinkTextDiv.html(this.blinkTexts[this.blinkCount++]);
                blinkTextDiv.fadeIn(300).animate({ opacity: 1.0 }).fadeOut(300, function () { return _this.blinkText(); });
            };
            return Tools;
        }());
        function Load() {
            window['R136JS'] = new Tools();
        }
        Interop.Load = Load;
    })(Interop = R136.Interop || (R136.Interop = {}));
})(R136 || (R136 = {}));
R136.Interop.Load();
//# sourceMappingURL=app.js.map