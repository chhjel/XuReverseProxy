<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject, Watch, Ref } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import NotificationRuleService from "@services/NotificationRuleService";
import StringUtils from "@utils/StringUtils";
import { NotificationRule } from "@generated/Models/Core/NotificationRule";
import CustomRequestDataEditor from "@components/inputs/CustomRequestDataEditor.vue";
import { CustomRequestData } from "@generated/Models/Core/CustomRequestData";
import {
  PlaceholderInfo,
  PlaceholderGroupInfo,
  getPlaceholdersForNotificationType,
  NotificationAlertTypeOptions,
  NotificationAlertTypeOption,
  NotificationTriggerOption,
  NotificationTriggerOptions,
} from "@utils/Constants";
import { NotificationAlertType } from "@generated/Enums/Core/NotificationAlertType";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import DateFormats from "@utils/DateFormats";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import TimeSpanInputComponent from "@components/inputs/TimeSpanInputComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    LoaderComponent,
    CheckboxComponent,
    CustomRequestDataEditor,
    ExpandableComponent,
    PlaceholderInfoComponent,
    TimeSpanInputComponent,
  },
})
export default class NotificationPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  service: NotificationRuleService = new NotificationRuleService();
  ruleId: string | null = null;
  rule: NotificationRule | null = null;

  webHookRequestData: CustomRequestData = {
    body: "",
    headers: "",
    requestMethod: "GET",
    url: "",
  };
  notificationAlertTypeOptions: Array<NotificationAlertTypeOption> = NotificationAlertTypeOptions;
  notificationTriggerOptions: Array<NotificationTriggerOption> = NotificationTriggerOptions;
  @Ref() readonly distinctPatternInput!: any;

  async mounted() {
    this.ruleId = StringUtils.firstOrDefault(this.$route.params.notificationId);
    const result = await this.service.GetAsync(this.ruleId);
    if (!result.success) {
      console.error(result.message);
    } else {
      this.rule = result.data;
      this.onRuleLoaded();
    }
  }

  onRuleLoaded(): void {
    this.webHookRequestData.url = this.rule.webHookUrl;
    this.webHookRequestData.requestMethod = this.rule.webHookMethod;
    this.webHookRequestData.headers = this.rule.webHookHeaders;
    this.webHookRequestData.body = this.rule.webHookBody;
  }

  @Watch("webHookRequestData", { deep: true })
  emitLocalValue(): void {
    this.rule.webHookUrl = this.webHookRequestData.url;
    this.rule.webHookMethod = this.webHookRequestData.requestMethod;
    this.rule.webHookHeaders = this.webHookRequestData.headers;
    this.rule.webHookBody = this.webHookRequestData.body;
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  placeholdersExtra: Array<PlaceholderInfo> = [];
  get placeholders(): Array<PlaceholderGroupInfo> {
    return getPlaceholdersForNotificationType(this.rule.triggerType);
  }

  get showWebhook(): boolean {
    return this.rule.alertType == NotificationAlertType.WebHook;
  }

  async saveRule() {
    const result = await this.service.CreateOrUpdateAsync(this.rule);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.rule = result.data;
    }
  }

  async deleteRule() {
    if (!confirm("Delete this rule?")) return;

    const result = await this.service.DeleteAsync(this.rule.id);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.$router.push({ name: "notifications" });
    }
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }

  insertDistinctPatterPlaceholder(val: string): void {
    this.distinctPatternInput.insertText(val);
  }
}
</script>

<template>
  <div class="notification-page">
    <loader-component :status="service.status" />
    <div v-if="service.status.hasDoneAtLeastOnce">
      <div v-if="rule" class="block">
        <div class="block-title">Notification rule</div>

        <checkbox-component
          label="Enabled"
          offLabel="Disabled"
          v-model:value="rule.enabled"
          class="mt-1 mb-2"
          :disabled="isLoading"
          warnColorOff
        />
        <text-input-component label="Name" v-model:value="rule.name" :disabled="isLoading" />
        <time-span-input-component
          class="mt-2"
          label="Cooldown"
          description="Determines how long the trigger is ignored for after each notification."
          emptyIsNull="true"
          v-model:value="rule.cooldown"
          :disabled="isLoading"
        />
        <text-input-component
          label="Cooldown distinct pattern"
          description="Each distinct pattern will have its own cooldown."
          v-model:value="rule.cooldownDistinctPattern"
          v-show="rule.cooldown"
          :disabled="isLoading"
          ref="distinctPatternInput"
        />
        <expandable-component header="Supported placeholders" v-if="rule.cooldown">
          <placeholder-info-component
            :placeholders="placeholders"
            :additionalPlaceholders="placeholdersExtra"
            @insertPlaceholder="insertDistinctPatterPlaceholder"
          />
        </expandable-component>

        <div class="mt-2">
          <label class="mr-2">Trigger on</label>
          <select v-model="rule.triggerType" :disabled="isLoading">
            <option v-for="triggerType in notificationTriggerOptions" :value="triggerType.value">
              {{ triggerType.name }}
            </option>
          </select>
        </div>

        <div class="mt-2">
          <label class="mr-2">Notify using</label>
          <select v-model="rule.alertType" :disabled="isLoading">
            <option v-for="alertType in notificationAlertTypeOptions" :value="alertType.value">
              {{ alertType.name }}
            </option>
          </select>
        </div>

        <CustomRequestDataEditor
          v-if="showWebhook"
          v-model:value="webHookRequestData"
          :placeholders="placeholders"
          :additionalPlaceholders="placeholdersExtra"
        />
      </div>

      <div class="block mt-2 mb-2 pb-1" v-if="rule && (rule.lastNotifiedAtUtc || rule.lastNotifyResult)">
        <div v-if="rule.lastNotifiedAtUtc" class="flexbox center-vertical">
          Last notified at:
          <code class="pa-0 ml-1">{{ formatDate(rule.lastNotifiedAtUtc) }}</code>
        </div>
        <div v-if="rule.lastNotifyResult" class="flexbox center-vertical">
          Last notify result:
          <code class="overflow-x-scroll pa-0 ml-1">'{{ rule.lastNotifyResult }}'</code>
        </div>
      </div>

      <div class="mt-1">
        <button-component @click="saveRule" :disabled="isLoading" class="ml-0 mr-1">Save</button-component>
        <button-component @click="deleteRule" :disabled="isLoading" class="ml-0 danger">Delete</button-component>
        <loader-component :status="service.status" inline inlineYAdjustment="-4px" />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.notification-page {
  padding-top: 20px;
}
</style>
