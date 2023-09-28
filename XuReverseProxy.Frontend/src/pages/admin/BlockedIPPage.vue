<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import StringUtils from "@utils/StringUtils";
import CustomRequestDataEditor from "@components/inputs/CustomRequestDataEditor.vue";
import { BlockedIpDataTypeOption, BlockedIpDataTypeOptions, NotificationAlertTypeOption } from "@utils/Constants";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import DateFormats from "@utils/DateFormats";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import IPBlockService from "@services/IPBlockService";
import { BlockedIpData } from "@generated/Models/Core/BlockedIpData";
import { BlockedIpDataType } from "@generated/Enums/Core/BlockedIpDataType";
import MiscUtilsService from "@services/MiscUtilsService";
import DateTimeInputComponent from "@components/inputs/DateTimeInputComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    LoaderComponent,
    CheckboxComponent,
    CustomRequestDataEditor,
    ExpandableComponent,
    PlaceholderInfoComponent,
    DateTimeInputComponent,
  },
})
export default class BlockedIPPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  miscUtilsService: MiscUtilsService = new MiscUtilsService();
  service: IPBlockService = new IPBlockService();
  ruleId: string | null = null;
  rule: BlockedIpData | null = null;

  blockTypeOptions: Array<BlockedIpDataTypeOption> = BlockedIpDataTypeOptions;
  testIp: string = "";
  testResult: string = "";
  testResultClasses: any = {};

  async mounted() {
    this.ruleId = StringUtils.firstOrDefault(this.$route.params.blockedId);
    const result = await this.service.GetAsync(this.ruleId);
    if (!result.success) {
      console.error(result.message);
    } else {
      this.rule = result.data;
    }
  }

  get isLoading(): boolean {
    return this.service.status.inProgress || this.miscUtilsService.status.inProgress;
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
    if (!confirm("Delete this ip block rule?")) return;

    const result = await this.service.DeleteAsync(this.rule.id);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.$router.push({ name: "blocked-ips" });
    }
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }

  get ipTestInputLabel(): string {
    if (this.rule.type == BlockedIpDataType.CIDRRange) return "IP to test CIDR range against";
    else if (this.rule.type == BlockedIpDataType.IP) return "IP to compare against";
    else if (this.rule.type == BlockedIpDataType.IPRegex) return "IP to test RegEx pattern against";
    else return "?";
  }

  async testRule() {
    this.testResultClasses = {};
    this.testResult = "";
    let isMatch = false;

    try {
      if (this.rule.type == BlockedIpDataType.CIDRRange) {
        isMatch = await this.miscUtilsService.IsIPInCidrRangeAsync({
          ip: this.testIp,
          ipCidr: this.rule.cidrRange,
        });
        this.testResult = isMatch ? "IP is within CIDR range." : "IP is not within CIDR range.";
      } else if (this.rule.type == BlockedIpDataType.IP) {
        isMatch = this.testIp?.toLowerCase()?.trim() == this.rule.ip?.toLowerCase()?.trim();
        this.testResult = isMatch ? "IPs match." : "IPs do not match.";
      } else if (this.rule.type == BlockedIpDataType.IPRegex) {
        isMatch = await this.miscUtilsService.TestRegexAsync({
          input: this.testIp,
          pattern: this.rule.ipRegex,
        });
        this.testResult = isMatch ? "IP matches RegEx." : "IP does not match RegEx.";
      }
    } catch (err) {
      this.testResult = err;
    }

    this.testResultClasses["success"] = isMatch;
  }
}
</script>

<template>
  <div class="blocked-ip-page">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <div v-if="rule" class="block">
        <div class="block-title">IP block</div>

        <checkbox-component
          label="Enabled"
          offLabel="Disabled"
          v-model:value="rule.enabled"
          class="mt-1 mb-2"
          :disabled="isLoading"
          warnColorOff
        />
        <text-input-component label="Name" v-model:value="rule.name" :disabled="isLoading" />
        <text-input-component label="Note" v-model:value="rule.note" :disabled="isLoading" />
        <date-time-input-component
          label="Blocked until"
          v-model:value="rule.blockedUntilUtc"
          :disabled="isLoading"
          emptyIsNull="true"
        />

        <div class="block block--dark mt-4">
          <label class="block-title">Block type</label>
          <div class="mt-1 mb-2">
            <select v-model="rule.type" :disabled="isLoading">
              <option v-for="blockType in blockTypeOptions" :value="blockType.value">
                {{ blockType.name }}
              </option>
            </select>
          </div>

          <text-input-component label="IP" v-model:value="rule.ip" v-if="rule.type == 'IP'" :disabled="isLoading" />
          <text-input-component
            label="IP RegEx"
            v-model:value="rule.ipRegex"
            v-if="rule.type == 'IPRegex'"
            :disabled="isLoading"
          />
          <text-input-component
            label="CIDR range"
            v-model:value="rule.cidrRange"
            v-if="rule.type == 'CIDRRange'"
            :disabled="isLoading"
          />
        </div>

        <div class="block block--dark mt-4">
          <label class="block-title">IP block test</label>
          <text-input-component :label="ipTestInputLabel" v-model:value="testIp" :disabled="isLoading" />
          <button-component @click="testRule" :disabled="isLoading" class="ml-0 mr-1" secondary>Test</button-component>
          <div class="ip-test-result" :class="testResultClasses" v-if="testResult">
            {{ testResult }}
          </div>
          <loader-component :status="service.status" inline inlineYAdjustment="-4px" />
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
.blocked-ip-page {
  padding-top: 20px;

  .ip-test-result {
    display: inline-block;
    font-family: monospace;
    border: 1px solid;
    padding: 10px;
    font-weight: 600;
    color: var(--color--warning-base);
    border-color: var(--color--warning-base);

    &.success {
      color: var(--color--success-base);
      border-color: var(--color--success-base);
    }
  }
}
</style>
