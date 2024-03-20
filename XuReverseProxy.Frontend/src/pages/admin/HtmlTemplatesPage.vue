<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject, Ref } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import { SortBy } from "@utils/SortUtils";
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
export default class HtmlTemplatesPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  @Ref() readonly htmlEditors!: any;

  service: HtmlTemplatesService = new HtmlTemplatesService();
  templates: Array<HtmlTemplate> = [];

  clientBlockedHtmlPlaceholders: Array<PlaceholderGroupInfo> = ClientBlockedHtmlPlaceholders;
  ipBlockedHtmlPlaceholders: Array<PlaceholderGroupInfo> = IPBlockedHtmlPlaceholders;
  html404Placeholders: Array<PlaceholderGroupInfo> = Html404Placeholders;
  proxyConditionsNotMetHtmlPlaceholders: Array<PlaceholderGroupInfo> = ProxyConditionsNotMetHtmlPlaceholders;

  async mounted() {
    const result = await this.service.GetAllAsync();
    if (!result.success) {
      console.error(result.message);
    }

    this.templates = (result.data || [])
      .filter(x => x.proxyConfigId == null)
      .sort((a, b) => SortBy(a, b, (x) => this.getTemplateOrder(x)));
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  async saveTemplate(template: HtmlTemplate) {
    await this.service.CreateOrUpdateAsync(template);
  }

  getTemplateOrder(template: HtmlTemplate): number {
    let index = this.templateOrder.indexOf(template.type);
    if (index == -1) return 99;
    else return this.templateOrder.length - index;
  }
  templateOrder: Array<HtmlTemplateType> = [
    HtmlTemplateType.ProxyNotFound,
    HtmlTemplateType.ClientBlocked,
    HtmlTemplateType.IPBlocked,
    HtmlTemplateType.ProxyConditionsNotMet,
  ];

  getTemplateName(template: HtmlTemplate): string {
    if (template.type == HtmlTemplateType.ClientBlocked) return "Client blocked response";
    else if (template.type == HtmlTemplateType.IPBlocked) return "IP blocked response";
    else if (template.type == HtmlTemplateType.ProxyConditionsNotMet) return "Proxy conditions not met response";
    else if (template.type == HtmlTemplateType.ProxyNotFound) return "Proxy not found response";
    else return template.type;
  }

  getPlaceholdersFor(type: HtmlTemplateType): Array<PlaceholderGroupInfo> {
    if (type == HtmlTemplateType.ClientBlocked) return this.clientBlockedHtmlPlaceholders;
    else if (type == HtmlTemplateType.IPBlocked) return this.ipBlockedHtmlPlaceholders;
    else if (type == HtmlTemplateType.ProxyConditionsNotMet) return this.proxyConditionsNotMetHtmlPlaceholders;
    else if (type == HtmlTemplateType.ProxyNotFound) return this.html404Placeholders;
    else return [];
  }

  insertPlaceholder(val: string, type: HtmlTemplateType): void {
    console.log(`todo: insert '${val}' into '${type}''`);
    console.log(this.htmlEditors);
    // this.notFoundHtmlEditor.insertText(val);
  }
}
</script>

<template>
  <div class="html-templates-page">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce || !service.status.success" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <div v-if="templates.length == 0 && service.status.done">- No templates found -</div>

      <div v-for="template in templates" :key="template.id" class="template block block--dark mt-2">
        <div class="block-title">{{ getTemplateName(template) }}</div>

        <code-input-component
          v-model:value="template.html"
          language="html"
          class="mt-2"
          height="400px"
          :readOnly="isLoading"
          ref="htmlEditors"
        />
        <expandable-component header="Supported placeholders">
          <placeholder-info-component
            :placeholders="getPlaceholdersFor(template.type)"
            @insertPlaceholder="(x) => insertPlaceholder(x, template.type)"
          />
        </expandable-component>

        <text-input-component
          label="Response code"
          v-model:value="template.responseCode"
          class="blocked-response-code-input mt-2"
        />

        <button-component primary @click="saveTemplate(template)" :disabled="isLoading" class="ml-0 mt-2"
          >Save</button-component
        >
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.html-templates-page {
  padding-top: 20px;

  .blocked-response-code-input {
    max-width: 200px;
  }
}
</style>
