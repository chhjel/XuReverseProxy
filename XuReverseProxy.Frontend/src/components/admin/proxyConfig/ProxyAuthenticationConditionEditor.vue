<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyAuthConditionTypeOption, ProxyAuthConditionTypeOptions } from "@utils/Constants";
import { ProxyAuthenticationCondition } from "@generated/Models/Core/ProxyAuthenticationCondition";
import { createProxyAuthenticationConditionSummary } from "@utils/ProxyAuthenticationConditionUtils";
import { DayOfWeekFlags } from "@generated/Enums/Core/DayOfWeekFlags";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyAuthenticationConditionEditor extends Vue {
  	@Prop()
	value: ProxyAuthenticationCondition;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyAuthenticationCondition | null = null;
	conditionTypeOptions: Array<ProxyAuthConditionTypeOption> = ProxyAuthConditionTypeOptions;

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
    @Watch('value')
    updateLocalValue(): void
    {
        const localJson = this.localValue ? JSON.stringify(this.localValue) : '';
        const valueJson = this.value ? JSON.stringify(this.value) : '';
		const changed = localJson != valueJson;
		if (changed) this.localValue = JSON.parse(valueJson);
    }

    @Watch('localValue', { deep: true })
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.updateLocalValue();
            return;
        }

        if (!this.localValue.dateTimeUtc1) this.localValue.dateTimeUtc1 = null;
        if (!this.localValue.dateTimeUtc2) this.localValue.dateTimeUtc2 = null;
        if (!this.localValue.timeOnlyUtc1) this.localValue.timeOnlyUtc1 = null;
        if (!this.localValue.timeOnlyUtc2) this.localValue.timeOnlyUtc2 = null;
        if (!this.localValue.daysOfWeekUtc) this.localValue.daysOfWeekUtc = DayOfWeekFlags.None;
		this.$emit('update:value', this.localValue);
    }
}
</script>

<template>
	<div class="proxyconfigauthchallenge-edit" v-if="localValue">
		<select v-model="localValue.conditionType" class="mb-2 mt-2">
			<option v-for="challengeType in conditionTypeOptions" 
				:value="challengeType.value">{{ challengeType.name }}</option>
		</select>

        <div v-if="localValue.conditionType == 'DateTimeRange'">
		    <text-input-component label="From" v-model:value="localValue.dateTimeUtc1" />
		    <text-input-component label="To" v-model:value="localValue.dateTimeUtc2" />
        </div>
        <div v-else-if="localValue.conditionType == 'TimeRange'">
		    <text-input-component label="From" v-model:value="localValue.timeOnlyUtc1" />
		    <text-input-component label="To" v-model:value="localValue.timeOnlyUtc2" />
        </div>
        <div v-else-if="localValue.conditionType == 'WeekDays'">
		    <text-input-component label="Weekdays" v-model:value="localValue.daysOfWeekUtc" />
        </div>

        <div class="mt-3">
            Authorization is required:
            <div><code style="font-size: 16px;" class="ml-2">{{ createAuthCondSummary(localValue) }}</code></div>
        </div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfigauthchallenge-edit {

}
</style>
