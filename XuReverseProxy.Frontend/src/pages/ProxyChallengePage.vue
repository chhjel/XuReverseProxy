<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import ProxyChallengeTypeManualApprovalComponent from "@components/proxyChallenges/ProxyChallengeTypeManualApprovalComponent.vue";
import { ProxyChallengePageFrontendModel } from "@generated/Models/Web/ProxyChallengePageFrontendModel";
import { ChallengeModel } from "@generated/Models/Web/ChallengeModel";
import { ProxyAuthenticationConditionType } from "@generated/Enums/Core/ProxyAuthenticationConditionType";
import ProxyChallengeTypeLoginComponent from "@components/proxyChallenges/ProxyChallengeTypeLoginComponent.vue";
import ProxyChallengeTypeOTPComponent from "@components/proxyChallenges/ProxyChallengeTypeOTPComponent.vue";
import ProxyChallengeTypeSecretQueryStringComponent from "@components/proxyChallenges/ProxyChallengeTypeSecretQueryStringComponent.vue";

interface AuthWithUnfulfilledConditions {
	name: string;
	fulfilledConditions: MaybeUnfulfilledCondition[];
	unfulfilledConditions: MaybeUnfulfilledCondition[];
}
interface MaybeUnfulfilledCondition {
	name: string;
	summary: string;
}

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		ProxyChallengeTypeManualApprovalComponent,
		ProxyChallengeTypeLoginComponent,
		ProxyChallengeTypeOTPComponent,
		ProxyChallengeTypeSecretQueryStringComponent
	}
})
export default class ProxyChallengePage extends Vue {
  	@Prop()
	options: ProxyChallengePageFrontendModel;
	
	solvedChallengeIds: Set<string> = new Set<string>();

	async mounted() {
	}

	get uncompletedChallenges(): Array<ChallengeModel> {
		return this.options.challengeModels.filter(x => !x.solved && !this.solvedChallengeIds.has(x.authId)).sort();
	}

	get completedChallenges(): Array<ChallengeModel> {
		return this.options.challengeModels.filter(x => x.solved || this.solvedChallengeIds.has(x.authId));
	}

	get unfulfilledAuths(): Array<AuthWithUnfulfilledConditions> {
		return this.options.authsWithUnfulfilledConditions.map(x => ({
			name: x.typeId.replace('ProxyChallengeType', ''),
			fulfilledConditions: x.conditions.filter(c => c.passed).map(c => ({
				name: c.type,
				summary: c.summary
			})),
			unfulfilledConditions: x.conditions.filter(c => !c.passed).map(c => ({
				name: c.type,
				summary: c.summary
			}))
		}));
	}

	getConditionTypeName(type: ProxyAuthenticationConditionType): string {
		if (type == ProxyAuthenticationConditionType.DateTimeRange) return 'Date';
		else if (type == ProxyAuthenticationConditionType.TimeRange) return 'Time of day';
		else if (type == ProxyAuthenticationConditionType.WeekDays) return 'Weekdays';
		else return '?'
	}

	getChallengeTypeIdName(typeId: string): string {
		return typeId.replace('ProxyChallengeType', '');
	}
		
	onChallengeSolved(challenge: ChallengeModel) {
		this.solvedChallengeIds.add(challenge.authId);
	}
}
</script>

<template>
	<div class="proxy-challenge-page">
		
		<div class="header" v-if="options.title || options.description">
			<h1 class="proxy-title" v-if="options.title">{{ options.title }}</h1>
			<div class="proxy-description" v-if="options.description">{{ options.description }}</div>
		</div>

		<div class="challenges mt-4" v-if="uncompletedChallenges.length > 0">
			<div v-for="challenge in uncompletedChallenges" class="challenges__item">
				<component :is="`${challenge.typeId}Component`" :options="challenge.frontendModel" @solved="onChallengeSolved(challenge)" />
			</div>
		</div>
		
		<div class="challenges-completed mt-4" v-if="completedChallenges.length > 0">
			<div class="challenges-completed__title">Completed challenges</div>
			<div v-for="challenge in completedChallenges" class="challenges-completed__item">
				<div class="material-icons icon">done</div>
				<div>{{ getChallengeTypeIdName(challenge.typeId) }}</div>
			</div>
		</div>

		<div class="challenges-unfulfilled mt-4" v-if="unfulfilledAuths.length > 0">
			<div class="challenges-unfulfilled__title">Challenges currently not required</div>
			<div v-for="auth in unfulfilledAuths"
				class="challenges-unfulfilled__item">
				<b>{{ auth.name }}</b>
				<div v-for="cond in auth.unfulfilledConditions" class="challenges-unfulfilled__conditionrow unfulfilled">
					<div class="material-icons icon">close</div>
					<div>{{ cond.summary }}</div>
				</div>
				<div v-for="cond in auth.fulfilledConditions" class="challenges-unfulfilled__conditionrow fulfilled">
					<div class="material-icons icon">done</div>
					<div>{{ cond.summary }}</div>
				</div>
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
			border: 2px solid var(--color--secondary);
			padding: 15px 20px 15px 20px;
			margin-top: 10px;
			background-color: var(--color--panel);
		}
	}

	.challenges-completed {
		border: 2px solid var(--color--panel);
		color: #757575;
		padding: 20px;

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

	.challenges-unfulfilled {
		border: 2px solid var(--color--panel);
		color: #757575;
		padding: 20px;

		&__title {
			font-weight: 600;
    		font-size: 18px;
		}

		&__item {
			margin-top: 10px;
		}

		&__conditionrow {
			display: flex;
    		align-items: center;

			.icon {
				margin-right: 2px;
			}
			&.fulfilled {
				.icon {
					color: var(--color--success-base);
				}
			}

			&.unfulfilled {
				.icon {
					color: var(--color--warning-base);
				}
			}
		}
	}
}
</style>
