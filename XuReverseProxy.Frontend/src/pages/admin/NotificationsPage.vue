<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import NotificationRuleService from "@services/NotificationRuleService";
import { NotificationRule } from "@generated/Models/Core/NotificationRule";
import ServerConfigService from "@services/ServerConfigService";
import { EmptyGuid, NotificationAlertTypeOptions, NotificationTriggerOptions } from "@utils/Constants";
import { NotificationTrigger } from "@generated/Enums/Core/NotificationTrigger";
import { NotificationAlertType } from "@generated/Enums/Core/NotificationAlertType";
import DateFormats from "@utils/DateFormats";
import { SortByThenBy } from "@utils/SortUtils";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
  },
})
export default class NotificationsPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  service: NotificationRuleService = new NotificationRuleService();
  rules: Array<NotificationRule> = [];
  globalAlertsEnabled: boolean | null = null;

  async mounted() {
    const result = await this.service.GetAllAsync();
    if (!result.success) {
      console.error(result.message);
    }
    this.rules = result.data || [];

    this.globalAlertsEnabled = await new ServerConfigService().IsConfigFlagEnabledAsync("EnableNotifications");
  }

  get sortedRules(): Array<NotificationRule> {
    return this.rules.sort((a, b) =>
      SortByThenBy(
        a,
        b,
        (x) => x.enabled,
        (x) => x.name,
        (a, b) => <any>b.enabled - <any>a.enabled,
        (a, b) => a.name?.localeCompare(b.name),
      ),
    );
  }

  getRuleStatus(rule: NotificationRule): string {
    if (this.globalAlertsEnabled === false) return "(notifications disabled in server config)";
    else if (!rule.enabled) return "(disabled)";
    else "";
  }

  getRuleIcon(rule: NotificationRule): any {
    if (!rule.enabled) return "notifications_off";
    else if (rule.lastNotifiedAtUtc) return "notifications_active";
    else return "notifications";
  }

  getRuleIconClasses(rule: NotificationRule): any {
    let classes: any = {};
    if (!rule.enabled || this.globalAlertsEnabled === false) classes["disabled"] = true;
    return classes;
  }

  async addNewRule() {
    let rule: NotificationRule = {
      id: EmptyGuid,
      enabled: false,
      name: "New Rule",
      triggerType: NotificationTrigger.AdminLoginSuccess,
      alertType: NotificationAlertType.WebHook,
      webHookUrl: "",
      webHookMethod: "GET",
      webHookHeaders: "",
      webHookBody: "",
      cooldownDistinctPattern: "",
      cooldown: "00:00:01",
      lastNotifiedAtUtc: null,
      lastNotifyResult: "",
    };
    const result = await this.service.CreateOrUpdateAsync(rule);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      rule = result.data;
      this.rules.push(rule);
      this.$router.push({
        name: "notification",
        params: { notificationId: rule.id },
      });
    }
  }

  getTriggerName(type: NotificationTrigger): string {
    return NotificationTriggerOptions.find((x) => x.value == type)?.name || "[unknown]";
  }
  getAlertName(type: NotificationAlertType): string {
    return NotificationAlertTypeOptions.find((x) => x.value == type)?.name || "[unknown]";
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }
}
</script>

<template>
  <div class="notifications-page">
    <loader-component :status="service.status" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <div v-if="sortedRules.length == 0 && service.status.done">- No rules configured yet -</div>
      <div v-for="rule in sortedRules" :key="rule.id">
        <router-link :to="{ name: 'notification', params: { notificationId: rule.id } }" class="rule">
          <div class="rule__header">
            <div class="material-icons icon" :class="getRuleIconClasses(rule)">
              {{ getRuleIcon(rule) }}
            </div>
            <div class="rule__name">
              {{ rule.name }}
              <span class="rule__status">{{ getRuleStatus(rule) }}</span>
            </div>
          </div>
          <div class="rule__summary">
            <span class="mr-2">On</span>
            <code>{{ getTriggerName(rule.triggerType) }}</code>
            <span class="ml-2 mr-2">notify using</span>
            <code>{{ getAlertName(rule.alertType) }}</code>
          </div>
          <div class="rule__lastresult" v-if="rule.lastNotifiedAtUtc">
            <span class="mr-2">Last notified at</span>
            <code>{{ formatDate(rule.lastNotifiedAtUtc) }}</code>
            <code class="overflow-ellipsis no-wrap" v-if="rule.lastNotifyResult"> * {{ rule.lastNotifyResult }}</code>
          </div>
        </router-link>
      </div>
      <button-component @click="addNewRule" v-if="service.status.done" class="primary ml-0"
        >Add new rule</button-component
      >
    </div>
  </div>
</template>

<style scoped lang="scss">
.notifications-page {
  padding-top: 20px;

  .rule {
    display: block;
    padding: 5px;
    margin: 5px 0;

    &:hover {
      text-decoration: none;
      background-color: var(--color--hover-bg);
    }

    &__header {
      display: flex;
      align-items: center;
    }

    &__status {
      font-size: 12px;
      color: var(--color--text-darker);
    }

    &__summary {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      font-size: 12px;
      color: var(--color--secondary);
      margin-left: 31px;
    }

    &__lastresult {
      color: var(--color--secondary);
      font-size: 12px;
      margin-left: 31px;
    }

    .icon {
      width: 24px;
      margin-right: 5px;
      color: var(--color--primary-lighten);

      &.disabled {
        color: var(--color--warning-base);
      }
    }
  }
}
</style>
