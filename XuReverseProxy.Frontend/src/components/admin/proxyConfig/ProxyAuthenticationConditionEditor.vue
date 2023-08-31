<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyAuthChallengeTypes } from "@utils/Constants";
import { ProxyAuthenticationCondition } from "@generated/Models/Core/ProxyAuthenticationCondition";
import { ProxyAuthenticationConditionType } from "@generated/Enums/Core/ProxyAuthenticationConditionType";

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
	challengeTypeOptions: Array<any> = ProxyAuthChallengeTypes;

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

		this.$emit('update:value', this.localValue);
    }
}
</script>

<template>
	<div class="proxyconfigauthchallenge-edit" v-if="localValue">
		<code>{{ localValue }}</code>
	</div>
</template>

<style scoped lang="scss">
.proxyconfigauthchallenge-edit {

}
</style>
