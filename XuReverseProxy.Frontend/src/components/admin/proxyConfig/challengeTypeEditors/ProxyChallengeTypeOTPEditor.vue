<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Ref } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeOTP } from "@generated/Models/Core/ProxyChallengeTypeOTP";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import { PlaceholderInfo, PlaceholderGroupInfo, OTPRequestUrlPlaceholders } from "@utils/Constants";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
        CodeInputComponent,
		ExpandableComponent,
		PlaceholderInfoComponent
	}
})
export default class ProxyChallengeTypeOTPEditor extends Vue {
  	@Prop()
	value: string;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyChallengeTypeOTP | null = null;
    @Ref() readonly urlEditor!: any;
    urlPlaceholdersExtra: Array<PlaceholderInfo> = [{
		name: "code",
		description: "Generated one-time code."
	}];
	urlPlaceholders: Array<PlaceholderGroupInfo> = OTPRequestUrlPlaceholders;

    mounted(): void {
        this.updateLocalValue();
        if (!this.localValue.webHookRequestMethod) this.localValue.webHookRequestMethod = 'GET';
        this.emitLocalValue();
    }

    insertUrlPlaceholder(val: string): void {
        this.urlEditor.insertText(val);
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
        <p>When the user clicks the button to send a one-time code a request is sent to the webhook url.</p>
		<text-input-component label="Description" v-model:value="localValue.description" :disabled="disabled" />
		<text-input-component label="WebHook request method" v-model:value="localValue.webHookRequestMethod" :disabled="disabled" class="mt-2" />
        <code-input-component v-model:value="localValue.webHookUrl" language="" :disabled="disabled" class="mt-2"
            height="100px" :wordWrap="true" label="WebHook url" ref="urlEditor" :readOnly="disabled" />
        <expandable-component header="Supported placeholders">
            <placeholder-info-component
                urlPlaceholdersExtra
                :placeholders="urlPlaceholders" 
                :additionalPlaceholders="urlPlaceholdersExtra" 
                @insertPlaceholder="insertUrlPlaceholder" />
        </expandable-component>
	</div>
</template>

<style scoped lang="scss">
/* .proxy-challenge-otp-edit { } */
</style>
