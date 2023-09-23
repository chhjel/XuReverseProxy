<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";
import { GlobalVariable } from "@generated/Models/Core/GlobalVariable";
import GlobalVariablesService from "@services/GlobalVariablesService";

@Options({
  components: {},
})
export default class PlaceholderInfoComponent extends Vue {
  @Prop()
  placeholders: Array<PlaceholderGroupInfo>;

  @Prop({ required: false, default: () => [] })
  additionalPlaceholders: Array<PlaceholderInfo>;

  allPlaceholderData: Array<PlaceholderInfo> = [];

  service: GlobalVariablesService = new GlobalVariablesService();
  globalVariables: Array<GlobalVariable> = [];
  isLoadingVariables: boolean = true;

  async mounted() {
    const variablesResult = await this.service.GetAllAsync();
    this.isLoadingVariables = false;
    if (variablesResult.success) {
      this.globalVariables = variablesResult.data;
    }

    this.rebuildData();
  }

  onInsertClicked(data: PlaceholderInfo) {
    this.$emit("insertPlaceholder", `{{${data.name}}}`);
  }

  rebuildData(): void {
    this.allPlaceholderData = [];
    this.additionalPlaceholders.forEach((d) => {
      this.allPlaceholderData.push(d);
    });
    this.placeholders.forEach((p) => {
      p.placeholders.forEach((d) => {
        this.allPlaceholderData.push(d);
      });
    });
    this.globalVariables.forEach((v) => {
        this.allPlaceholderData.push({
          name: v.name,
          description: "Custom variable"
        });
    });
  }

  @Watch("placeholders", { deep: true })
  onPlaceholdersChanged(): void {
    this.rebuildData();
  }

  @Watch("additionalPlaceholders", { deep: true })
  onAdditionalPlaceholdersChanged(): void {
    this.rebuildData();
  }
}
</script>

<template>
  <div class="placeholder-details">
    <table>
      <tr>
        <th>Name</th>
        <th>Description</th>
      </tr>
      <tr v-for="data in allPlaceholderData">
        <td>
          <a @click.prevent.stop="onInsertClicked(data)" href="#"
            ><code>&#123;&#123;{{ data.name }}&#125;&#125;</code></a
          >
        </td>
        <td>{{ data.description }}</td>
      </tr>
    </table>
  </div>
</template>

<style scoped lang="scss">
.placeholder-details {
  overflow-x: auto;
  max-height: 300px;
  padding-right: 5px;

  table {
    text-align: left;
  }
  th {
    padding: 3px;
  }
  td {
    color: var(--color--text-dark);
    padding: 3px;
  }
  tr {
    border-bottom: 1px solid var(--color--text-darker);
    padding: 3px;

    &:nth-child(odd) {
      background-color: var(--color--table-odd);
    }
  }
}
</style>
