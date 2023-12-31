<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import ProxyChallengeTypeManualApprovalComponent from "@components/proxyChallenges/ProxyChallengeTypeManualApprovalComponent.vue";
import { ProxyChallengePageFrontendModel } from "@generated/Models/Web/ProxyChallengePageFrontendModel";
import { ChallengeModel } from "@generated/Models/Web/ChallengeModel";
import ProxyChallengeTypeLoginComponent from "@components/proxyChallenges/ProxyChallengeTypeLoginComponent.vue";
import ProxyChallengeTypeAdminLoginComponent from "@components/proxyChallenges/ProxyChallengeTypeAdminLoginComponent.vue";
import ProxyChallengeTypeOTPComponent from "@components/proxyChallenges/ProxyChallengeTypeOTPComponent.vue";
import ProxyChallengeTypeSecretQueryStringComponent from "@components/proxyChallenges/ProxyChallengeTypeSecretQueryStringComponent.vue";
import { nextTick } from "vue";
import { getProxyAuthenticationTypeName } from "@utils/ProxyAuthenticationDataUtils";
import ConditionsStateSummary from "@components/common/ConditionsStateSummary.vue";
import { ConditionStateSummaryItem } from "@components/common/ConditionsStateSummary.Models";

interface AuthWithUnfulfilledConditions {
  name: string;
  fulfilledConditions: MaybeUnfulfilledCondition[];
  unfulfilledConditions: MaybeUnfulfilledCondition[];
}
interface MaybeUnfulfilledCondition {
  name: string;
  summary: string;
  group: number;
}

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    ProxyChallengeTypeManualApprovalComponent,
    ProxyChallengeTypeLoginComponent,
    ProxyChallengeTypeAdminLoginComponent,
    ProxyChallengeTypeOTPComponent,
    ProxyChallengeTypeSecretQueryStringComponent,
    ConditionsStateSummary,
  },
})
export default class ProxyChallengePage extends Vue {
  @Prop()
  options: ProxyChallengePageFrontendModel;

  solvedChallengeIds: Set<string> = new Set<string>();

  async mounted() {}

  getConditionSummaryFor(auth: AuthWithUnfulfilledConditions): Array<ConditionStateSummaryItem> {
    const conditions: Array<ConditionStateSummaryItem> = [];
    auth.fulfilledConditions.forEach((x) => {
      conditions.push({
        group: x.group,
        summary: x.summary,
        completed: true,
      });
    });
    auth.unfulfilledConditions.forEach((x) => {
      conditions.push({
        group: x.group,
        summary: x.summary,
        completed: false,
      });
    });
    return conditions;
  }

  get uncompletedChallenges(): Array<ChallengeModel> {
    return this.options.challengeModels.filter((x) => !x.solved && !this.solvedChallengeIds.has(x.authId)).sort();
  }

  get completedChallenges(): Array<ChallengeModel> {
    return this.options.challengeModels.filter((x) => x.solved || this.solvedChallengeIds.has(x.authId));
  }

  get unfulfilledAuths(): Array<AuthWithUnfulfilledConditions> {
    return this.options.authsWithUnfulfilledConditions.map((x) => ({
      name: this.getChallengeTypeIdName(x.typeId),
      fulfilledConditions: x.conditions
        .filter((c) => c.passed)
        .map((c) => ({
          name: c.type,
          summary: c.summary,
          group: c.group,
        })),
      unfulfilledConditions: x.conditions
        .filter((c) => !c.passed)
        .map((c) => ({
          name: c.type,
          summary: c.summary,
          group: c.group,
        })),
    }));
  }

  getChallengeTypeIdName(typeId: string): string {
    return getProxyAuthenticationTypeName(typeId);
  }

  onChallengeSolved(challenge: ChallengeModel) {
    this.solvedChallengeIds.add(challenge.authId);

    nextTick(() => {
      if (this.uncompletedChallenges.length == 0) {
        setTimeout(() => {
          window.location.reload();
        }, 1000);
      }
    });
  }
}
</script>

<template>
  <div class="proxy-challenge-page">
    <div class="header" v-if="options.title || options.description">
      <h1 class="proxy-title" v-if="options.title">{{ options.title }}</h1>
      <div class="proxy-description" v-if="options.description">
        {{ options.description }}
      </div>
    </div>

    <div v-if="uncompletedChallenges.length == 0" class="block granted">
      - Access granted -
      <div class="mt-3">Loading..</div>
    </div>

    <div class="challenges mt-4" v-if="uncompletedChallenges.length > 0">
      <div v-for="challenge in uncompletedChallenges" class="challenges__item block block--secondary">
        <component
          :is="`${challenge.typeId}Component`"
          :options="challenge.frontendModel"
          @solved="onChallengeSolved(challenge)"
        />
      </div>
    </div>

    <div class="challenges-completed mt-4 block block--dark" v-if="completedChallenges.length > 0">
      <div class="challenges-completed__title">Completed challenges</div>
      <div v-for="challenge in completedChallenges" class="challenges-completed__item">
        <div class="material-icons icon">done</div>
        <div>{{ getChallengeTypeIdName(challenge.typeId) }}</div>
      </div>
    </div>

    <div class="challenges-unfulfilled mt-4 block block--dark" v-if="unfulfilledAuths.length > 0">
      <div class="challenges-unfulfilled__title">Challenges currently not required</div>
      <div v-for="auth in unfulfilledAuths" class="challenges-unfulfilled__item">
        <b>{{ auth.name }}</b>
        <conditions-state-summary class="challenges-unfulfilled__summary" :value="getConditionSummaryFor(auth)" />
      </div>
    </div>
  </div>
</template>

<style lang="scss">
.challenge-header {
  display: flex;
  align-items: center;
  /* justify-content: center; */

  .challenge-title {
    font-weight: 600;
    color: var(--color--warning-base);
  }
  .icon {
    margin-right: 5px;
    color: var(--color--warning-base);
  }
}
</style>
<style scoped lang="scss">
.proxy-challenge-page {
  max-width: 600px;
  margin: auto;
  padding: 40px;
  @media (max-width: 600px) {
    padding: 10px;
  }

  .header {
    margin-bottom: 40px;
  }

  .proxy-title {
    margin-top: 0;
  }

  .proxy-description {
    color: var(--color--text-dark);
  }

  .challenges {
    &__title {
      font-weight: 600;
      font-size: 18px;
    }

    &__item {
      margin-top: 10px;
      margin-bottom: 20px;
    }
  }

  .challenges-completed {
    &__title {
      font-weight: 600;
      font-size: 18px;
    }

    &__item {
      margin-top: 10px;
      display: flex;
      align-items: center;
    }

    .icon {
      margin-right: 2px;
      color: var(--color--success-base);
    }
  }

  .granted {
    text-align: center;
    color: var(--color--success-base);
    font-size: 20px;
    font-weight: 600;

    div {
      font-size: 16px;
    }
  }

  .challenges-unfulfilled {
    &__title {
      font-weight: 600;
      font-size: 18px;
    }

    &__item {
      margin-top: 10px;
    }

    &__summary {
      margin-top: 10px;
      margin-left: 10px;
    }
  }
}
</style>
