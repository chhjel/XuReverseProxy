import { GlobalVariable } from "@generated/Models/Core/GlobalVariable";
import EFCrudServiceBase from "./EFCrudServiceBase";

export default class GlobalVariablesService extends EFCrudServiceBase<GlobalVariable> {
  constructor() {
    super("globalVariables");
  }
}
