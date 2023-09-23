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

  async mounted() {
    const result = await this.service.GetAllAsync();
    if (!result.success) {
      console.error(result.message);
    }
    this.variables = result.data || [];
  }

  get sortedVariables(): Array<GlobalVariable> {
    return this.variables.sort((a, b) =>
      SortBy(
        a,
        b,
        (x) => x.name,
        (a, b) => a.name?.localeCompare(b.name),
      ),
    );
  }

  async addNewVariable() {
    let variable: GlobalVariable = {
      id: EmptyGuid,
      name: "NewVariable",
      value: "",
      lastUpdatedAtUtc: null,
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
      this.$router.push({
        name: "notification",
        params: { notificationId: variable.id },
      });
    }
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }
}
</script>

<template>
  <div class="variables-page">
    <loader-component :status="service.status" />
    <div v-if="service.status.hasDoneAtLeastOnce">
      <div v-if="sortedVariables.length == 0 && service.status.done">- No variables added yet -</div>

      <div v-for="variable in sortedVariables" :key="variable.id" class="variable">
        <code>{{ variable }}</code>
      </div>

      <button-component @click="addNewVariable" v-if="service.status.done" class="primary ml-0"
        >Add new variable</button-component
      >
    </div>
  </div>
</template>

<style scoped lang="scss">
.variables-page {
  padding-top: 20px;

  .variable {
    display: block;
    padding: 5px;
    margin: 5px 0;
  }
}
</style>
