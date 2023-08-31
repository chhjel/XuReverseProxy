<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeOTP } from "@generated/Models/Core/ProxyChallengeTypeOTP";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeOTPEditor extends Vue {
  	@Prop()
	value: string;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyChallengeTypeOTP | null= null;

    mounted(): void {
        this.updateLocalValue();
        if (!this.localValue.webHookRequestMethod) this.localValue.webHookRequestMethod = 'GET';
        this.emitLocalValue();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		const changed = JSON.stringify(this.localValue) != this.value;
		if (changed) this.localValue = JSON.parse(this.value);
    }

    @Watch('localValue', { deep: true })
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.updateLocalValue();
            return;
        }

		this.$emit('update:value', JSON.stringify(this.localValue));
    }
}
</script>

<template>
	<div class="proxy-challenge-otp-edit" v-if="localValue">
		<text-input-component label="Description" v-model:value="localValue.description" />
		<text-input-component label="WebHook request method" v-model:value="localValue.webHookRequestMethod" />
		<text-input-component label="WebHook url" v-model:value="localValue.webHookUrl" />
        <p>Use placeholder <code>&#123;&#123;code&#125;&#125;</code> for the generated code.</p>
	</div>
</template>

<style scoped lang="scss">
.proxy-challenge-otp-edit {

}
</style>
