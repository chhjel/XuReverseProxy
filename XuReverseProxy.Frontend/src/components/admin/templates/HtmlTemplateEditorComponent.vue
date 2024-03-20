<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Ref, Prop, Watch } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import HtmlTemplatesService from "@services/HtmlTemplatesService";
import { HtmlTemplate } from "@generated/Models/Core/HtmlTemplate";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";
import { HtmlTemplateType } from "@generated/Enums/Core/HtmlTemplateType";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import {
  ClientBlockedHtmlPlaceholders,
  Html404Placeholders,
  IPBlockedHtmlPlaceholders,
  PlaceholderGroupInfo,
  ProxyConditionsNotMetHtmlPlaceholders,
} from "@utils/Constants";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
    CodeInputComponent,
    ExpandableComponent,
    PlaceholderInfoComponent,
  },
})
export default class HtmlTemplateEditorComponent extends Vue {
  @Prop()
  value: HtmlTemplate;

  @Prop({ required: false, default: false })
  disabled: boolean;

  @Ref() readonly htmlEditor!: any;

  localValue: HtmlTemplate | null = null;
  service: HtmlTemplatesService = new HtmlTemplatesService();

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue();
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  get isDisabled(): boolean {
    return this.isLoading || this.disabled;
  }

  async saveTemplate(template: HtmlTemplate) {
    await this.service.CreateOrUpdateAsync(template);
  }

  getTemplateName(template: HtmlTemplate): string {
    if (template.type == HtmlTemplateType.ClientBlocked) return "Client blocked response";
    else if (template.type == HtmlTemplateType.IPBlocked) return "IP blocked response";
    else if (template.type == HtmlTemplateType.ProxyConditionsNotMet) return "Proxy conditions not met response";
    else if (template.type == HtmlTemplateType.ProxyNotFound) return "Proxy not found response";
    else return template.type;
  }

  getPlaceholdersFor(type: HtmlTemplateType): Array<PlaceholderGroupInfo> {
    if (type == HtmlTemplateType.ClientBlocked) return ClientBlockedHtmlPlaceholders;
    else if (type == HtmlTemplateType.IPBlocked) return IPBlockedHtmlPlaceholders;
    else if (type == HtmlTemplateType.ProxyConditionsNotMet) return ProxyConditionsNotMetHtmlPlaceholders;
    else if (type == HtmlTemplateType.ProxyNotFound) return Html404Placeholders;
    else return [];
  }

  insertPlaceholder(val: string, type: HtmlTemplateType): void {
    if (this.isDisabled) return;
    this.htmlEditor.insertText(val);
  }
  
  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    const changed = JSON.stringify(this.localValue) != JSON.stringify(this.value);
    if (changed) this.localValue = JSON.parse(JSON.stringify(this.value));
  }

  @Watch("localValue", { deep: true })
  emitLocalValue(): void {
    if (this.disabled) {
      this.updateLocalValue();
      return;
    }

    this.$emit("update:value", this.localValue);
  }
}
</script>

<template>
  <div class="html-template-editor-component">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce || !service.status.success" />

    <div v-if="localValue">
      <div class="title">{{ getTemplateName(localValue) }}</div>
  
      <code-input-component
          v-model:value="localValue.html"
          language="html"
          class="mt-0"
          height="400px"
          :readOnly="isDisabled"
          ref="htmlEditor"
      />
      <expandable-component header="Supported placeholders">
          <placeholder-info-component
          :placeholders="getPlaceholdersFor(localValue.type)"
          @insertPlaceholder="(x) => insertPlaceholder(x, localValue.type)"
          />
      </expandable-component>
  
      <text-input-component
          label="Response code"
          v-model:value="localValue.responseCode"
          :readOnly="isDisabled"
          class="blocked-response-code-input mt-2"
      />
  
      <button-component primary @click="saveTemplate(localValue)" :disabled="isDisabled" class="ml-0 mt-2"
          >Save</button-component
      >
    </div>
  </div>
</template>

<style scoped lang="scss">
.html-template-editor-component {
  /* padding-top: 20px; */

  .title {
	font-size: 22px;
	color: var(--color--text-dark);
  }

  .blocked-response-code-input {
    max-width: 200px;
  }
}
</style>
