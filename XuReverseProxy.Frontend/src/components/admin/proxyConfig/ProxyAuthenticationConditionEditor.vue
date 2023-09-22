<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyAuthConditionTypeOption, ProxyAuthConditionTypeOptions, WeekdayOptions } from "@utils/Constants";
import { ProxyAuthenticationCondition } from "@generated/Models/Core/ProxyAuthenticationCondition";
import { createProxyAuthenticationConditionSummary } from "@utils/ProxyAuthenticationConditionUtils";
import { DayOfWeekFlags } from "@generated/Enums/Core/DayOfWeekFlags";
import TimeOnlyInputComponent from "@components/inputs/TimeOnlyInputComponent.vue";
import DateTimeInputComponent from "@components/inputs/DateTimeInputComponent.vue";
import { MultiCheckboxComponentOption } from "@components/inputs/MultiCheckboxComponent.Models";
import MultiCheckboxComponent from "@components/inputs/MultiCheckboxComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    TimeOnlyInputComponent,
    DateTimeInputComponent,
    MultiCheckboxComponent
  },
})
export default class ProxyAuthenticationConditionEditor extends Vue {
  @Prop()
  value: ProxyAuthenticationCondition;

  @Prop({ required: false, default: false })
  disabled: boolean;

  localValue: ProxyAuthenticationCondition | null = null;
  conditionTypeOptions: Array<ProxyAuthConditionTypeOption> = ProxyAuthConditionTypeOptions;
  
  weekDaysValue: Array<MultiCheckboxComponentOption> = [];
  weekdayOptions: Array<DayOfWeekFlags> = WeekdayOptions;

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue();
  }

  createAuthCondSummary(cond: ProxyAuthenticationCondition): string {
    return createProxyAuthenticationConditionSummary(cond);
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

    const flaggedValues = this.localValue.daysOfWeekUtc.split(',').map(x => x.trim());
    this.weekDaysValue = this.weekdayOptions.map(x => {
      return {
        onLabel: x,
        offLabel: x,
        value: flaggedValues.includes(x)
      };
    })
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
    this.$emit("update:value", this.localValue);
  }

  onWeekdaysChanged(): void {
    this.localValue.daysOfWeekUtc = <DayOfWeekFlags>this.weekDaysValue
      .filter(x => x.value)
      .map(x => x.onLabel).join(', ');
  }
}
</script>

<template>
  <div class="proxyconfigauthchallenge-edit" v-if="localValue">
    <select v-model="localValue.conditionType" class="mb-2 mt-2" :disabled="disabled">
      <option v-for="challengeType in conditionTypeOptions" :value="challengeType.value">
        {{ challengeType.name }}
      </option>
    </select>

    <div v-if="localValue.conditionType == 'DateTimeRange'">
      <date-time-input-component label="From" v-model:value="localValue.dateTimeUtc1" :disabled="disabled" />
      <date-time-input-component label="To" v-model:value="localValue.dateTimeUtc2" :disabled="disabled" />
    </div>
    <div v-else-if="localValue.conditionType == 'TimeRange'">
      <time-only-input-component label="From" v-model:value="localValue.timeOnlyUtc1" :disabled="disabled" />
      <time-only-input-component label="To" v-model:value="localValue.timeOnlyUtc2" :disabled="disabled" />
    </div>
    <div v-else-if="localValue.conditionType == 'WeekDays'">
      <multi-checkbox-component v-model:value="weekDaysValue" :disabled="disabled" @change="onWeekdaysChanged" />
    </div>

    <div class="mt-3">
      Authentication is required:
      <div>
        <code style="font-size: 16px" class="ml-2">{{ createAuthCondSummary(localValue) }}</code>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.proxyconfigauthchallenge-edit {
}
</style>
