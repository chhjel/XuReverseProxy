<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import { GlobalVariable } from "@generated/Models/Core/GlobalVariable";
import { EmptyGuid } from "@utils/Constants";
import DateFormats from "@utils/DateFormats";
import { SortBy } from "@utils/SortUtils";
import GlobalVariablesService from "@services/GlobalVariablesService";
import StringUtils from "@utils/StringUtils";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
  },
})
export default class GlobalVariablesPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  service: GlobalVariablesService = new GlobalVariablesService();
  variables: Array<GlobalVariable> = [];
  savedVariableState: Array<GlobalVariable> = [];

  async mounted() {
    const result = await this.service.GetAllAsync();
    if (!result.success) {
      console.error(result.message);
    }

    this.variables = (result.data || []).sort((a, b) =>
      SortBy(
        a,
        b,
        (x) => x.name,
        (a, b) => a.name?.localeCompare(b.name),
      ),
    );
    this.updateSavedVariableState();
  }

  updateSavedVariableState(): void {
    this.savedVariableState = JSON.parse(JSON.stringify(this.variables));
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  get highlightedId(): string | null {
    return StringUtils.firstOrDefault(this.$route.params.variableId);
  }

  async addNewVariable() {
    let variable: GlobalVariable = {
      id: EmptyGuid,
      name: "NewVariable",
      value: "",
      lastUpdatedAtUtc: new Date(),
      lastUpdatedBy: null,
      lastUpdatedSourceIP: null,
    };
    const result = await this.service.CreateOrUpdateAsync(variable);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      variable = result.data;
      this.variables.push(variable);
      this.savedVariableState.push(JSON.parse(JSON.stringify(variable)));
      this.$router.push({
        name: "variables",
        params: { variableId: variable.id },
      });
    }
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }

  getVariableClasses(variable: GlobalVariable): any {
    return {
      highlighted: this.highlightedId == variable.id,
    };
  }

  isVariableChanged(variable: GlobalVariable): boolean {
    const stateItem = this.savedVariableState.find((x) => x.id == variable.id);
    return stateItem?.name !== variable.name || stateItem?.value !== variable.value;
  }

  variableSaveButtonTooltip(variable: GlobalVariable): string {
    if (!this.isVariableChanged(variable)) return "Nothing to save, change the name or value first.";
    else return "";
  }

  async saveVariable(variable: GlobalVariable) {
    const result = await this.service.CreateOrUpdateAsync(variable);
    if (result.success) {
      const stateItem = this.savedVariableState.find((x) => x.id == variable.id);
      stateItem.name = variable.name;
      stateItem.value = variable.value;
    }
  }

  async tryDeleteVariable(variable: GlobalVariable) {
    if (!confirm(`Delete variable '${variable.name}'?`)) return;
    const result = await this.service.DeleteAsync(variable.id);
    if (result.success) {
      this.variables = this.variables.filter((x) => x.id != variable.id);
    }
  }
}
</script>

<template>
  <div class="variables-page">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce || !service.status.success" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <p>Variables can be used in any placeholders, and support up to 3 levels of recursion.</p>
      <div class="table-wrapper">
        <table>
          <tr>
            <th>Name</th>
            <th>Value</th>
            <th></th>
            <th></th>
          </tr>
          <tr v-for="variable in variables" :key="variable.id" class="variable" :class="getVariableClasses(variable)">
            <td class="variable__name">
              <text-input-component v-model:value="variable.name" dark :disabled="isLoading" />
            </td>
            <td class="variable__value">
              <text-input-component v-model:value="variable.value" dark textarea rows="3" :disabled="isLoading" />
            </td>
            <td class="variable__actions">
              <button-component
                primary
                small
                @click="saveVariable(variable)"
                :disabled="isLoading || !isVariableChanged(variable)"
                :title="variableSaveButtonTooltip(variable)"
                >Save</button-component
              >
              <button-component danger small @click="tryDeleteVariable(variable)" :disabled="isLoading"
                >Delete</button-component
              >
            </td>
            <td class="variable__details">
              Last updated
              {{ formatDate(variable.lastUpdatedAtUtc) }}
              by <b>{{ variable.lastUpdatedBy }}</b> from {{ variable.lastUpdatedSourceIP }}
            </td>
          </tr>
        </table>
      </div>
      
      <div v-if="variables.length == 0 && service.status.done">- No variables added yet -</div>

      <button-component @click="addNewVariable" v-if="service.status.done" class="primary ml-0"
        >Add new variable</button-component
      >
    </div>
  </div>
</template>

<style scoped lang="scss">
.variables-page {
  padding-top: 20px;

  .table-wrapper {
    overflow-x: auto;
    padding-bottom: 5px;
  }

  table {
    width: 100%;

    th {
      text-align: left;
    }

    textarea {
      width: 100%;
    }
  }

  .variable {
    padding: 5px;
    margin: 5px 0;

    &__name {
      position: relative;
      min-width: 200px;

      .input-wrapper {
        position: absolute;
        top: 0;
        bottom: 0;
        left: 0;
        right: 0;
      }
    }

    &__details {
      font-size: 11px;
      color: var(--color--secondary);
      width: 115px;
      min-width: 115px;
      font-family: monospace;
    }

    &__actions {
      width: 150px;
      min-width: 150px;
    }
  }
}
</style>
