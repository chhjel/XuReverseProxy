<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ConditionTypeOption, ConditionTypeOptions, WeekdayOptions } from "@utils/Constants";
import { ConditionData } from "@generated/Models/Core/ConditionData";
import { createConditionDataSummary } from "@utils/ConditionDataUtils";
import { DayOfWeekFlags } from "@generated/Enums/Core/DayOfWeekFlags";
import TimeOnlyInputComponent from "@components/inputs/TimeOnlyInputComponent.vue";
import DateTimeInputComponent from "@components/inputs/DateTimeInputComponent.vue";
import { MultiCheckboxComponentOption } from "@components/inputs/MultiCheckboxComponent.Models";
import MultiCheckboxComponent from "@components/inputs/MultiCheckboxComponent.vue";
import RegexTestComponent from "@components/common/RegexTestComponent.vue";
import CidrTestComponent from "@components/common/CidrTestComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    TimeOnlyInputComponent,
    DateTimeInputComponent,
    MultiCheckboxComponent,
    CidrTestComponent,
    RegexTestComponent,
  },
})
export default class ConditionDataEditor extends Vue {
  @Prop()
  value: ConditionData;

  @Prop({ required: false, default: false })
  disabled: boolean;

  @Prop({ required: false, default: "Condition passes:" })
  summaryLabel: string;

  localValue: ConditionData | null = null;
  conditionTypeOptions: Array<ConditionTypeOption> = ConditionTypeOptions;

  weekDaysValue: Array<MultiCheckboxComponentOption> = [];
  weekdayOptions: Array<DayOfWeekFlags> = WeekdayOptions;

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue();
  }

  createAuthCondSummary(cond: ConditionData): string {
    return createConditionDataSummary(cond);
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    const localJson = this.localValue ? JSON.stringify(this.localValue) : "";
    const valueJson = this.value ? JSON.stringify(this.value) : "";
    const changed = localJson != valueJson;
    if (changed) this.localValue = JSON.parse(valueJson);

    const flaggedValues = this.localValue.daysOfWeekUtc?.split(",").map((x) => x.trim()) || [];
    this.weekDaysValue = this.weekdayOptions.map((x) => {
      return {
        onLabel: x,
        offLabel: x,
        value: flaggedValues.includes(x),
      };
    });
  }

  @Watch("localValue", { deep: true })
  emitLocalValue(): void {
    if (this.disabled) {
      this.updateLocalValue();
      return;
    }

    if (!this.localValue.dateTimeUtc1) this.localValue.dateTimeUtc1 = null;
    if (!this.localValue.dateTimeUtc2) this.localValue.dateTimeUtc2 = null;
    if (!this.localValue.timeOnlyUtc1) this.localValue.timeOnlyUtc1 = null;
    if (!this.localValue.timeOnlyUtc2) this.localValue.timeOnlyUtc2 = null;
    if (!this.localValue.daysOfWeekUtc) this.localValue.daysOfWeekUtc = DayOfWeekFlags.None;
    if (!this.localValue.ipCondition) this.localValue.ipCondition = null;
    this.$emit("update:value", this.localValue);
  }

  onWeekdaysChanged(): void {
    this.localValue.daysOfWeekUtc = <DayOfWeekFlags>this.weekDaysValue
      .filter((x) => x.value)
      .map((x) => x.onLabel)
      .join(", ");
  }
}
</script>

<template>
  <div class="proxyconfigauthchallenge-edit" v-if="localValue">
    <text-input-component label="Condition group" v-model:value="localValue.group" :disabled="disabled" description="Determines what group to AND this condition with." type="number" />

    <select v-model="localValue.type" class="mb-2 mt-2" :disabled="disabled">
      <option v-for="challengeType in conditionTypeOptions" :value="challengeType.value">
        {{ challengeType.name }}
      </option>
    </select>

    <div v-if="localValue.type == 'DateTimeRange'">
      <date-time-input-component label="From" v-model:value="localValue.dateTimeUtc1" :disabled="disabled" />
      <date-time-input-component label="To" v-model:value="localValue.dateTimeUtc2" :disabled="disabled" />
    </div>
    <div v-else-if="localValue.type == 'TimeRange'">
      <time-only-input-component label="From" v-model:value="localValue.timeOnlyUtc1" :disabled="disabled" />
      <time-only-input-component label="To" v-model:value="localValue.timeOnlyUtc2" :disabled="disabled" />
    </div>
    <div v-else-if="localValue.type == 'WeekDays'">
      <multi-checkbox-component v-model:value="weekDaysValue" :disabled="disabled" @change="onWeekdaysChanged" />
    </div>
    <div v-else-if="localValue.type == 'IPEquals'">
      <text-input-component label="IP" v-model:value="localValue.ipCondition" :disabled="disabled" />
    </div>
    <div v-else-if="localValue.type == 'IPRegex'">
      <text-input-component label="RegEx pattern" v-model:value="localValue.ipCondition" :disabled="disabled" />
      <div class="block block--dark mt-4">
        <label class="block-title">RegEx test</label>
        <regex-test-component :value="localValue.ipCondition" />
      </div>
    </div>
    <div v-else-if="localValue.type == 'IPCIDRRange'">
      <text-input-component label="CIDR range" v-model:value="localValue.ipCondition" :disabled="disabled" />
      <div class="block block--dark mt-4">
        <label class="block-title">CIDR test</label>
        <regex-test-component :value="localValue.ipCondition" />
      </div>
    </div>
    <div v-else-if="localValue.type == 'IsLocalRequest'">
      <!-- No inputs needed here -->
    </div>
    <div v-else>
      <code>Todo: support condition type {{ localValue.type }}</code>
    </div>

    <div class="mt-3">
      {{ summaryLabel }}
      <div>
        <code style="font-size: 16px" class="ml-2">{{ createAuthCondSummary(localValue) }}</code>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
/* .proxyconfigauthchallenge-edit {} */
</style>
