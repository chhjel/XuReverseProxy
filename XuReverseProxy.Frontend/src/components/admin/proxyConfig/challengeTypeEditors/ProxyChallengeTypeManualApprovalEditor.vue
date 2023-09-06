<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Ref } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeManualApproval } from "@generated/Models/Core/ProxyChallengeTypeManualApproval";
import { ManualApprovalUrlPlaceholders, PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
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
export default class ProxyChallengeTypeManualApprovalEditor extends Vue {
  	@Prop()
	value: string;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyChallengeTypeManualApproval | null = null;
    @Ref() readonly urlEditor!: any;
    urlPlaceholdersExtra: Array<PlaceholderInfo> = [{
		name: "url",
		description: "The generated, escaped url of the page where you can approve the request."
	}];
	urlPlaceholders: Array<PlaceholderGroupInfo> = ManualApprovalUrlPlaceholders;

    mounted(): void {
        this.updateLocalValue();
        if (!this.localValue.webHookRequestMethod) this.localValue.webHookRequestMethod = 'GET';
        if (!this.localValue.webHookUrl) this.localValue.webHookUrl = 'https://www.your-notification-service.com?url={{url}}';
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
	<div class="proxy-challenge-manual-edit" v-if="localValue">
        <p>When the user clicks the button to request access, a request is sent to the webhook url.</p>
		<text-input-component label="WebHook request method" v-model:value="localValue.webHookRequestMethod" class="mt-2" />
        <code-input-component v-model:value="localValue.webHookUrl" language=""  class="mt-2"
            height="100px" :wordWrap="true" label="WebHook url" ref="urlEditor" />
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
.proxy-challenge-manual-edit {

}
</style>
