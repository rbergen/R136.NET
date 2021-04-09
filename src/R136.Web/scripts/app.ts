namespace R136.Interop {

  class Tools {

    public scrollToElementId(elementId: string): boolean {
      let element: HTMLElement = document.getElementById(elementId);

      if (!element)
        return false;

      element.scrollIntoView({ behavior: "smooth" });

      return true;
    }

    public adoptVerticalPadding(targetId: string, headerId: string, footerId: string): void {
      let target: HTMLElement = document.getElementById(targetId);
      let header: HTMLElement = document.getElementById(headerId);
      let footer: HTMLElement = document.getElementById(footerId);

      if (!target)
        return;

      if (header)
        target.style.paddingTop = header.offsetHeight + "px";

      if (footer)
        target.style.paddingBottom = footer.offsetHeight + "px";
    }

    public setClipboard(text: string): void {
      navigator.clipboard.writeText(text);
    }

    tooltipList: Bootstrap.Tooltip[];

    public enableTooltips() {
      this.tooltipList = Array.from(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map((element) => new Bootstrap.Tooltip(element));
    }

    public closeTooltips() {
      if (this.tooltipList)
        this.tooltipList.forEach((tooltip) => tooltip.hide());
    }

    blinkTexts: Array<string>;
    blinkCount: number;

    blinkInit(): boolean {
      if ($("#blinkTextMessage")) {
        $(".blinkTextContent").each((_index, element) => {
          this.blinkTexts.push($(element).text());
        });

        return true;
      }

      return false;
    }

    blinkText(): void {
      let blinkTextDiv = $("#blinkTextMessage");
      if (!blinkTextDiv)
        return;

      if (this.blinkCount >= this.blinkTexts.length)
        this.blinkCount = 0;

      blinkTextDiv.html(this.blinkTexts[this.blinkCount++]);
      blinkTextDiv.fadeIn(300).animate({ opacity: 1.0 }).fadeOut(300, () => this.blinkText());
    }

    constructor() {
      this.blinkTexts = []
      this.blinkCount = 0;

      if (this.blinkInit())
        this.blinkText();
    }
  }

  export function Load(): void {
    window['R136JS'] = new Tools();
	}

}

R136.Interop.Load();