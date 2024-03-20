import EFCrudServiceBase from "./EFCrudServiceBase";
import { HtmlTemplate } from "@generated/Models/Core/HtmlTemplate";

export default class HtmlTemplatesService extends EFCrudServiceBase<HtmlTemplate> {
  constructor() {
    super("htmlTemplates");
  }
}
