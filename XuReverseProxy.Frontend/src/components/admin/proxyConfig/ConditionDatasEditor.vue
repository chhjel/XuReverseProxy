<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from "vue-property-decorator";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ConditionData } from "@generated/Models/Core/ConditionData";
import ConditionDataEditor from "./ConditionDataEditor.vue";
import { ConditionTypeOptions, EmptyGuid } from "@utils/Constants";
import ConditionDataService from "@services/ConditionDataService";
import DialogComponent from "@components/common/DialogComponent.vue";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import IdUtils from "@utils/IdUtils";
import { createConditionDataSummary } from "@utils/ConditionDataUtils";

interface ConditionGroup {
  group: number;
  conditions: Array<ConditionData>;
}

@Options({
  components: {
    ButtonComponent,
    ConditionDataEditor,
    LoaderComponent,
    DialogComponent,
  },
})
export default class ConditionDatasEditor extends Vue {
  @Prop()
  parentId: string;

  @Prop()
  parentType: "ProxyConfig" | "ProxyAuthenticationData";

  @Prop()
  value: Array<ConditionData>;

  @Prop({ required: false, default: false })
  disabled: boolean;

  id: string = IdUtils.generateId();
  emptyGuid: string = EmptyGuid;
  conditionDialogVisible: boolean = false;
  conditionInDialog: ConditionData | null = null;
  conditionService: ConditionDataService = new ConditionDataService();

  mounted(): void {
    // this.updateLocalValue();
    // this.emitLocalValue();
  }

  get conditionGroups(): Array<ConditionGroup> {
    const map: { [key: string]: ConditionGroup } = {};

    const grouped = this.value.reduce((prev, x) => {
      prev[x.group] = prev[x.group] || {
        group: x.group,
        conditions: [],
      };

      prev[x.group].conditions.push(x);
      return prev;
    }, map);

    return Object.keys(grouped).map((x) => grouped[x]);
  }

  get isLoading(): boolean {
    return this.conditionService.status.inProgress;
  }

  showConditionDialog(cond: ConditionData) {
    this.conditionInDialog = cond;
    this.conditionDialogVisible = true;
  }

  async saveCondtion() {
    const result = await this.conditionService.CreateOrUpdateAsync(this.conditionInDialog, null, null, this.parentType);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.conditionInDialog = result.data;
      this.conditionDialogVisible = false;
      this.onConditionSaved(result.data);
    }
  }

  onConditionSaved(cond: ConditionData) {
    this.$emit("save", cond);
  }

  async deleteCondition() {
    const result = await this.conditionService.DeleteAsync(this.conditionInDialog.id);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.$emit("delete", this.conditionInDialog);
      this.conditionDialogVisible = false;
      this.conditionInDialog = null;
    }
  }

  async onAddConditionClicked(): Promise<any> {
    const cond: ConditionData = {
      id: EmptyGuid,
      parentId: this.parentId,
      group: 0,
      type: ConditionTypeOptions[0].value,
      dateTimeUtc1: null,
      dateTimeUtc2: null,
      daysOfWeekUtc: null,
      timeOnlyUtc1: null,
      timeOnlyUtc2: null,
      ipCondition: null,
    };
    this.showConditionDialog(cond);
  }

  createAuthCondSummary(cond: ConditionData): string {
    return createConditionDataSummary(cond);
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    // const localJson = this.localValue ? JSON.stringify(this.localValue) : "";
    // const valueJson = this.value ? JSON.stringify(this.value) : "";
    // const changed = localJson != valueJson;
    // if (changed) this.localValue = JSON.parse(valueJson);
  }

  @Watch("localValue", { deep: true })
  emitLocalValue(): void {
    // if (this.disabled) {
    //   this.updateLocalValue();
    //   return;
    // }
    // this.$emit("update:value", this.localValue);
  }
}
</script>

<template>
  <div class="conditions-edit">
    <div class="conditions-edit__overview" v-if="conditionGroups.length > 0">
      <div v-for="(group, gIndex) in conditionGroups" :key="`${id}-group-${group.group}`">
        <div class="conditions-edit__group">
          <div
            v-for="(cond, cIndex) in group.conditions"
            :key="`${id}-group-${group.group}-${cond.id}`"
            class="conditions-edit__condition"
          >
            <div class="conditions-edit__condition-link" @click="showConditionDialog(cond)">
              {{ createAuthCondSummary(cond) }}
            </div>
            <div v-if="cIndex < group.conditions.length - 1" class="conditions-edit__condition-and">AND</div>
          </div>
        </div>
        <div v-if="gIndex < conditionGroups.length - 1" class="conditions-edit__group-or">OR</div>
      </div>
    </div>

    <div>
      <button-component @click="onAddConditionClicked()" small secondary class="add-cond-button ml-0" icon="add"
        >Add condition</button-component
      >
    </div>

    <!-- Condition Dialog -->
    <dialog-component v-model:value="conditionDialogVisible" max-width="600" persistent>
      <template #header>Condition</template>
      <template #footer>
        <button-component @click="saveCondtion" :disabled="isLoading" class="primary ml-0">Save</button-component>
        <button-component @click="conditionDialogVisible = false" :disabled="isLoading" class="secondary"
          >Cancel</button-component
        >
        <button-component
          @click="deleteCondition"
          :disabled="isLoading"
          class="danger"
          v-if="conditionInDialog?.id != emptyGuid"
          >Delete</button-component
        >
        <loader-component :status="conditionService.status" inline inlineYAdjustment="-4px" />
      </template>
      <condition-data-editor
        v-if="conditionInDialog"
        :key="conditionInDialog.id"
        v-model:value="conditionInDialog"
        :disabled="isLoading"
        summaryLabel="Authentication is required:"
      />
    </dialog-component>
  </div>
</template>

<style scoped lang="scss">
.conditions-edit {
  &__overview {
    margin-bottom: 8px;
  }
  &__group {
    display: flex;
    flex-wrap: wrap;

    &-or {
      align-self: center;
      font-family: monospace;
      font-weight: 600;
      font-size: 14px;
      margin: 10px 0;
    }
  }
  &__condition {
    display: flex;
    margin-bottom: 3px;
    &-link {
      display: inline-flex;
      cursor: pointer;
      padding: 10px;
      background-color: var(--color--hover-bg);

      &:hover {
        text-decoration: underline;
      }
    }
    &-and {
      align-self: center;
      font-family: monospace;
      font-weight: 600;
      font-size: 14px;
      margin: 5px;
    }
  }
}
</style>
