<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import DialogComponent from "@components/common/DialogComponent.vue";
import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import { ProxyAuthChallengeTypeOption, ProxyAuthChallengeTypeOptions } from "@utils/Constants";
import ProxyChallengeTypeLoginEditor from "./challengeTypeEditors/ProxyChallengeTypeLoginEditor.vue";
import ProxyChallengeTypeAdminLoginEditor from "./challengeTypeEditors/ProxyChallengeTypeAdminLoginEditor.vue";
import ProxyChallengeTypeOTPEditor from "./challengeTypeEditors/ProxyChallengeTypeOTPEditor.vue";
import ProxyChallengeTypeManualApprovalEditor from "./challengeTypeEditors/ProxyChallengeTypeManualApprovalEditor.vue";
import ProxyChallengeTypeSecretQueryStringEditor from "./challengeTypeEditors/ProxyChallengeTypeSecretQueryStringEditor.vue";
import TimeSpanInputComponent from "@components/inputs/TimeSpanInputComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		DialogComponent,
		ProxyChallengeTypeLoginEditor,
		ProxyChallengeTypeAdminLoginEditor,
		ProxyChallengeTypeOTPEditor,
		ProxyChallengeTypeManualApprovalEditor,
		ProxyChallengeTypeSecretQueryStringEditor,
		TimeSpanInputComponent
	}
})
export default class ProxyAuthenticationDataEditor extends Vue {
  	@Prop()
	value: ProxyAuthenticationData;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyAuthenticationData | null = null;
	challengeTypeOptions: Array<ProxyAuthChallengeTypeOption> = ProxyAuthChallengeTypeOptions;

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		const changed = JSON.stringify(this.localValue) != JSON.stringify(this.value);
		if (changed) this.localValue = JSON.parse(JSON.stringify(this.value));
    }

    @Watch('localValue', { deep: true })
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.updateLocalValue();
            return;
        }

		this.$emit('update:value', this.localValue);
    }
}
</script>

<template>
	<div class="proxyconfigauth-edit" v-if="localValue">
		<select v-model="localValue.challengeTypeId" class="mb-2 mt-2" :disabled="disabled">
			<option v-for="challengeType in challengeTypeOptions" 
				:value="challengeType.typeId">{{ challengeType.name }}</option>
		</select>

		<component :is="`${localValue.challengeTypeId}Editor`"
			:disabled="disabled"
			v-model:value="localValue.challengeJson" />

		<div class="mt-3">
			<time-span-input-component
				label="Solved duration"
				description="Determines for how long the challenge will be solved. After the given duration the challenge will need to be solved again."
				noteIfNull="No duration configured - the challenge will be solved forever for the client."
				emptyIsNull="true" v-model:value="localValue.solvedDuration" :disabled="disabled" />
		</div>
	</div>
</template>

<style scoped lang="scss">
/* .proxyconfigauth-edit {

} */
</style>
