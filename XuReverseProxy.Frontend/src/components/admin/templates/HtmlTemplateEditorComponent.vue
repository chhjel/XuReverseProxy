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
import InputHeaderComponent from "@components/inputs/InputHeaderComponent.vue";
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
    InputHeaderComponent,
  },
})
export default class HtmlTemplateEditorComponent extends Vue {
  @Prop()
  value: HtmlTemplate;

  @Prop({ required: false, default: false })
  disabled: boolean;

  @Prop({ required: false, default: false })
  allowRemove: boolean;

  @Prop({ required: false, default: () => null })
  allowedTypes: Array<HtmlTemplateType> | null;

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

  get showTypes(): boolean {
    return this.allowedTypes != null && this.allowedTypes.length > 0;
  }

  get typeOptions(): Array<any> {
    return this.allowedTypes.map((x) => {
      return {
        name: this.getTemplateName(x),
        value: x,
      };
    });
  }

  async saveTemplate(template: HtmlTemplate) {
    await this.service.CreateOrUpdateAsync(template);
  }

  getTemplateName(type: HtmlTemplateType): string {
    if (type == HtmlTemplateType.ClientBlocked) return "Client blocked response";
    else if (type == HtmlTemplateType.IPBlocked) return "IP blocked response";
    else if (type == HtmlTemplateType.ProxyConditionsNotMet) return "Proxy conditions not met response";
    else if (type == HtmlTemplateType.ProxyNotFound) return "Proxy not found response";
    else return type;
  }

  getPlaceholdersFor(type: HtmlTemplateType): Array<PlaceholderGroupInfo> {
    if (type == HtmlTemplateType.ClientBlocked) return ClientBlockedHtmlPlaceholders;
    else if (type == HtmlTemplateType.IPBlocked) return IPBlockedHtmlPlaceholders;
    else if (type == HtmlTemplateType.ProxyConditionsNotMet) return ProxyConditionsNotMetHtmlPlaceholders;
    else if (type == HtmlTemplateType.ProxyNotFound) return Html404Placeholders;
    else return [];
  }

  insertPlaceholder(val: string): void {
    if (this.isDisabled) return;
    this.htmlEditor.insertText(val);
  }

  async onRemoveResponseOverrideClicked(): Promise<any> {
    if (!confirm(`Remove html template override for ${this.getTemplateName(this.localValue.type)}?`)) return;
    const result = await this.service.DeleteAsync(this.localValue.id);
    if (result.success) {
      this.$emit("onRemoved", this.localValue);
    }
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
      <div class="title" v-if="!showTypes">{{ getTemplateName(localValue.type) }}</div>

      <div v-if="showTypes">
        <input-header-component label="Type" />
        <select v-model="localValue.type" :disabled="disabled">
          <option v-for="type in typeOptions" :value="type.value">
            {{ type.name }}
          </option>
        </select>
      </div>

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
          @insertPlaceholder="(x) => insertPlaceholder(x)"
        />
      </expandable-component>

      <text-input-component
        label="Response code"
        v-model:value="localValue.responseCode"
        :readOnly="isDisabled"
        class="blocked-response-code-input mt-2"
      />

      <button-component primary @click="saveTemplate(localValue)" :disabled="isDisabled" class="ml-0 mt-3"
        >Save</button-component
      >
      <button-component @click="onRemoveResponseOverrideClicked()" danger class="ml-0 mt-3" v-if="allowRemove"
        >Remove</button-component
      >
    </div>
  </div>
</template>

<style scoped lang="scss">
.html-template-editor-component {
  /* padding-top: 20px; */

  .title {
    font-size: 16px;
    color: var(--color--text-dark);
  }

  .blocked-response-code-input {
    max-width: 200px;
  }
}
</style>
