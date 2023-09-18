<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Inject, Ref } from 'vue-property-decorator'
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";
import { CustomRequestData } from "@generated/Models/Core/CustomRequestData";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import { HttpRequestMethodOptions, PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";

@Options({
	components: {
		CodeInputComponent,
		ExpandableComponent,
		PlaceholderInfoComponent
	}
})
export default class CustomRequestDataEditor extends Vue {
  	@Prop()
	value: CustomRequestData;

  	@Prop({ required: false, default: false})
	disabled: boolean;
    
    @Prop()
    placeholders: Array<PlaceholderGroupInfo>;

    @Prop({ required: false, default: () => []})
    additionalPlaceholders: Array<PlaceholderInfo>
	
	localValue: CustomRequestData | null = null;
    httpRequestMethodOptions: Array<any> = HttpRequestMethodOptions;
    
    @Ref() readonly urlEditor!: any;
    @Ref() readonly headersEditor!: any;
    @Ref() readonly bodyEditor!: any;

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

    placeholderTarget: 'body' | 'headers' | 'url' | '' = '';
	insertPlaceholder(val: string): void {
        if (this.placeholderTarget == 'body') this.bodyEditor.insertText(val);
        else if (this.placeholderTarget == 'headers') this.headersEditor.insertText(val);
        else if (this.placeholderTarget == 'url') this.urlEditor.insertText(val);
	}
}
</script>

<template>
	<div class="request-data-edit" v-if="localValue">
		<div class="block block--dark mt-2">
            <div class="block-title">WebHook</div>
            <div class="mt-2">
                <label class="mr-2">Request method</label>
                <select v-model="localValue.requestMethod" :disabled="disabled">
                    <option v-for="methodOption in httpRequestMethodOptions" 
                        :value="methodOption.value">{{ methodOption.name }}</option>
                </select>
            </div>
			<code-input-component label="Url" language="" v-model:value="localValue.url" class="mt-2" height="100px"
                :readOnly="disabled" ref="urlEditor" @focus="placeholderTarget = 'url'" wordWrap="true" />
			<code-input-component label="Headers" language="" v-model:value="localValue.headers" class="mt-2" height="200px"
                :readOnly="disabled" ref="headersEditor" @focus="placeholderTarget = 'headers'"  />
            <code class="display-block">One header per line, e.g:</code>
            <code class="display-block">Accept: application/json</code>
            <code class="display-block">Content-Type: application/json</code>
			<code-input-component label="Body" v-model:value="localValue.body" language="json" class="mt-2" height="400px"
                :readOnly="disabled" ref="bodyEditor" @focus="placeholderTarget = 'body'"  />
            
            <expandable-component header="Supported placeholders (url, body & headers)">
                <placeholder-info-component
                    :placeholders="placeholders" 
                    :additionalPlaceholders="additionalPlaceholders" 
                    @insertPlaceholder="insertPlaceholder" />
            </expandable-component>
        </div>
	</div>
</template>

<style scoped lang="scss">
/* .request-data-edit {

} */
</style>
