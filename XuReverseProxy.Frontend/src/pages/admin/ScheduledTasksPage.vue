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

interface JobData {
	type: string;
	status: ScheduledTaskStatus | null;
	result: ScheduledTaskResult | null;
}

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
	statuses: Array<ScheduledTaskStatus> = [];
	results: Array<ScheduledTaskResult> = [];

	async mounted() {
		this.statuses = await this.service.GetTaskStatusesAsync();
		this.results = await this.service.GetTaskResultsAsync();
	}

	get jobDatas(): Array<JobData> {
		let datas: Array<JobData> = [];
		this.statuses.forEach(x => {
			let data = datas.find(d => d.type == x.jobTypeName);
			if (!data) {
				data = { type: x.jobTypeName, status: null, result: null };
				datas.push(data);
			}
			data.status = x;
		});
		this.results.forEach(x => {
			let data = datas.find(d => d.type == x.jobTypeName);
			if (!data) {
				data = { type: x.jobTypeName, status: null, result: null };
				datas.push(data);
			}
			data.result = x;
		});
		return datas;
	}

	formatDate(raw: Date | string): string {
		if (raw == null) return '';
		let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
		return date.toLocaleString();
	}
}
</script>

<template>
	<div class="scheduledtasks-page">
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
			<div v-for="data in jobDatas" class="job block mb-4">
				<div class="job__name">{{ data.type }}</div>
				<div class="job__chips" v-if="data.status">
					<div class="job__chip" v-if="data.status.isRunning">Running</div>
					<div class="job__chip" v-if="data.status.failed">Failed</div>
					<div class="job__chip" v-if="data.status.lastStartedAt">Started at {{ formatDate(data.status.lastStartedAt) }}</div>
					<div class="job__chip" v-if="data.status.stoppedAt">Stopped at {{ formatDate(data.status.stoppedAt) }}</div>
				</div>
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
			&.success { border-color: var(--color--success-base); }
			&.error { border-color: var(--color--error-base); }
		}
		&__result {
			margin-top: 20px;
			border: 1px solid var(--color--panel-light);
			padding: 10px;
			font-size: 14px;
			&.success { border-color: var(--color--success-base); }
			&.error { border-color: var(--color--error-base); }
		}
		&__result-time {
			margin-top: 5px;
		}
	}
}
</style>
