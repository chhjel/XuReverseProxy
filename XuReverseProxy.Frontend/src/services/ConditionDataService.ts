import { ConditionData } from "@generated/Models/Core/ConditionData";
import EFCrudServiceBase from "./EFCrudServiceBase";

export default class ConditionDataService extends EFCrudServiceBase<ConditionData> {
  constructor() {
    super("conditionData");
  }
}
