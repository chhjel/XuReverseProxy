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
import HtmlTemplateEditorComponent from "@components/admin/templates/HtmlTemplateEditorComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
    CodeInputComponent,
    ExpandableComponent,
    PlaceholderInfoComponent,
    HtmlTemplateEditorComponent,
  },
})
export default class HtmlTemplatesPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  @Ref() readonly htmlEditors!: any;

  service: HtmlTemplatesService = new HtmlTemplatesService();
  templates: Array<HtmlTemplate> = [];

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

  onTemplateUpdated(template: HtmlTemplate): void {
    const index = this.templates.findIndex(x => x.id == template.id);
    if (index != -1) this.templates[index] = template;
  }
}
</script>

<template>
  <div class="html-templates-page">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce || !service.status.success" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <div v-if="templates.length == 0 && service.status.done">- No templates found -</div>

      <div v-for="template in templates" :key="template.id" class="template block block--dark mt-2">
        <HtmlTemplateEditorComponent :value="template" :disabled="isLoading" @update:value="onTemplateUpdated(template)" />
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
