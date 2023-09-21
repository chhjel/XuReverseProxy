import { NotificationRule } from "../generated/Models/Core/NotificationRule";
import EFCrudServiceBase from "./EFCrudServiceBase";

export default class NotificationRuleService extends EFCrudServiceBase<NotificationRule> {
  constructor() {
    super("notificationRule");
  }
}
