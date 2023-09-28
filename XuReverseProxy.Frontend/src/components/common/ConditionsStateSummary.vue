<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from "vue-property-decorator";
import { ConditionStateSummaryItem } from "./ConditionsStateSummary.Models";

interface ConditionStateSummaryItemGroup {
  group: number;
  conditions: Array<ConditionStateSummaryItem>;
}

@Options({
  components: {},
})
export default class ConditionsStateSummary extends Vue {
  @Prop()
  value: Array<ConditionStateSummaryItem>;

  mounted(): void {}

  get conditionGroups(): Array<ConditionStateSummaryItemGroup> {
    const map: { [key: string]: ConditionStateSummaryItemGroup } = {};

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
}
</script>

<template>
  <div class="conditions-state" v-if="conditionGroups.length > 0">
    <div v-for="(group, gIndex) in conditionGroups">
      <div class="conditions-state__group">
        <div v-for="(cond, cIndex) in group.conditions" class="conditions-state__condition">
          <div class="conditions-state__condition-label">
            <div class="material-icons icon" v-if="!cond.completed">close</div>
            <div class="material-icons icon completed" v-if="cond.completed">done</div>
            {{ cond.summary }}
          </div>
          <div v-if="cIndex < group.conditions.length - 1" class="conditions-state__condition-and">AND</div>
        </div>
      </div>
      <div v-if="gIndex < conditionGroups.length - 1" class="conditions-state__group-or">OR</div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.conditions-state {
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

    &-label {
      display: inline-flex;
      align-items: center;
      padding: 5px;
      background-color: var(--color--hover-bg);

      .icon {
        color: var(--color--warning-base);
        &.completed {
          color: var(--color--success-base);
        }
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
