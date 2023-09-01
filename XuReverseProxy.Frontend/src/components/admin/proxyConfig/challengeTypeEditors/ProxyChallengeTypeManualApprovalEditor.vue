<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeManualApproval } from "@generated/Models/Core/ProxyChallengeTypeManualApproval";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeManualApprovalEditor extends Vue {
  	@Prop()
	value: string;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyChallengeTypeManualApproval | null= null;

    mounted(): void {
        this.updateLocalValue();
        if (!this.localValue.webHookRequestMethod) this.localValue.webHookRequestMethod = 'GET';
        if (!this.localValue.webHookUrl) this.localValue.webHookUrl = 'https://www.your-notification-service.com?url={{url}}';
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
	<div class="proxy-challenge-manual-edit" v-if="localValue">
        <p>When the user clicks the button to request access, a request is sent to the webhook url.</p>
		<text-input-component label="WebHook request method" v-model:value="localValue.webHookRequestMethod" />
		<text-input-component label="WebHook url" v-model:value="localValue.webHookUrl" />
        <p>Use placeholder <code>&#123;&#123;url&#125;&#125;</code> for the generated, escaped url of the page where you can approve the request.</p>
	</div>
</template>

<style scoped lang="scss">
.proxy-challenge-manual-edit {

}
</style>
