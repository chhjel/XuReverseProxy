<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import ScheduledTasksService from "@services/ScheduledTasksService";
import { ScheduledTaskStatus } from "@generated/Models/Core/ScheduledTaskStatus";
import { ScheduledTaskResult } from "@generated/Models/Core/ScheduledTaskResult";
import DateFormats from "@utils/DateFormats";
import { ScheduledTaskViewModel } from "@generated/Models/Web/ScheduledTaskViewModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		LoaderComponent
	}
})
export default class ScheduledTasksPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ScheduledTasksService = new ScheduledTasksService();
	jobDatas: Array<ScheduledTaskViewModel> = [];

	async mounted() {
		this.jobDatas = await this.service.GetTasksDetailsAsync();
	}

	formatDate(raw: Date | string): string {
		return DateFormats.defaultDateTime(raw);
	}
}
</script>

<template>
	<div class="scheduledtasks-page">
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
			<div v-for="data in jobDatas" class="job block mb-4">
				<div class="job__name">{{ data.name }}</div>
				<div class="job__chips" v-if="data.status">
					<div class="job__chip" v-if="data.status.isRunning">Running</div>
					<div class="job__chip" v-if="data.status.failed">Failed</div>
					<div class="job__chip" v-if="data.status.lastStartedAt">Started at {{ formatDate(data.status.lastStartedAt) }}</div>
					<div class="job__chip" v-if="data.status.stoppedAt">Stopped at {{ formatDate(data.status.stoppedAt) }}</div>
				</div>
				<p class="job__desc" v-if="data.description">{{ data.description }}</p>
				<div class="job__status" v-if="data.status" :class="{ success: !data.status.isRunning && !data.status.failed, error: data.status.failed }">
					<b>Last status:</b> {{ data.status.message }}
				</div>
				<div class="job__result" v-if="data.result" :class="{ success: data.result.success, error: !data.result.success }">
					<b>Last result</b> 
					<div class="job__result-time">Ran from {{ formatDate(data.result.startedAtUtc) }} to {{ formatDate(data.result.stoppedAtUtc) }}</div>
					<div class="job__result-message">{{ data.result.result }}</div>
					<div class="job__result-exception" v-if="data.result.exception"><code>{{ data.result.exception }}</code></div>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.scheduledtasks-page {
	padding-top: 20px;

	.job {
		&__name {
			font-size: 24px;
			margin-bottom: 10px;
		}
		&__desc {
			font-size: 12px;
			color: var(--color--text-dark);
		}
		&__chips {
			display: flex;
    		flex-wrap: wrap;
		}
		&__chip {
			margin-right: 10px;
			background-color: var(--color--panel-light);
			padding: 5px 8px;
			margin-bottom: 5px;
			border-radius: 12px;
			font-size: 12px;
			color: var(--color--text-dark);
		}
		&__status {
			margin-top: 15px;
			border: 1px solid var(--color--panel-light);
			padding: 10px;
			font-size: 14px;
			overflow-x: auto;
			&.success { border-color: var(--color--success-base); }
			&.error { border-color: var(--color--error-base); }
		}
		&__result {
			margin-top: 20px;
			border: 1px solid var(--color--panel-light);
			padding: 10px;
			font-size: 14px;
			overflow-x: auto;
			&.success { border-color: var(--color--success-base); }
			&.error { border-color: var(--color--error-base); }
		}
		&__result-time {
			margin-top: 5px;
		}
	}
}
</style>
