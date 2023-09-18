<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject, Ref } from 'vue-property-decorator'
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ServerLogService from "@services/ServerLogService";
import { LoggedEvent } from "@generated/Models/Core/LoggedEvent";
import DateFormats from "@utils/DateFormats";
import ServerConfigService from "@services/ServerConfigService";

@Options({
	components: {
	}
})
export default class ServerLogPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ServerLogService = new ServerLogService();
	entries: Array<LoggedEvent> = [];
	memoryLoggingEnabled: boolean | null = null;

    @Ref() readonly logContainer!: HTMLElement;


	async mounted() {
		this.entries = await this.service.GetLogAsync();
		this.memoryLoggingEnabled = await new ServerConfigService().IsConfigFlagEnabledAsync("EnableMemoryLogging");

		this.logContainer.scrollTop = this.logContainer.scrollHeight;
	}
	
	formatDate(raw: Date | string): string {
		return DateFormats.defaultDateTime(raw);
	}

	getRowClasses(e: LoggedEvent): any {
		return {
			trace: <any>e.logLevel == 'Trace',
			debug: <any>e.logLevel == 'Debug',
			information: <any>e.logLevel == 'Information',
			warning: <any>e.logLevel == 'Warning',
			error: <any>e.logLevel == 'Error',
			critical: <any>e.logLevel == 'Critical',
		};
	}
}
</script>

<template>
	<div class="log-page">
		<p v-if="memoryLoggingEnabled === false">- Memory logging is currently disabled -</p>
		<p v-else class="mt-0">Showing latest {{ entries.length }} log entries (max 1000).</p>
		<div class="log-entries block mb-0" ref="logContainer">
			<div v-for="entry in entries">
				<code class="log-row" :class="getRowClasses(entry)" >[{{ formatDate(entry.timestampUtc) }}] [{{ entry.logLevel }}] {{ entry.message }}</code>
				<code class="log-row" :class="getRowClasses(entry)" v-if="entry.exception">{{ entry.exception }}</code>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.log-page {
	padding-top: 20px;

	.log-entries {
		height: calc(100vh - 272px);
  		overflow: auto;
		
		@media (max-width: 869px) {
			height: calc(100vh - 250px);
		}

		>div:last-child {
			margin-bottom: 10%;
		}
	}

	.log-row {
		color: var(--color--text-dark);
		&.information { color: var(--color--info-base); }
		&.warning { color: var(--color--warning-base); }
		&.error { color: var(--color--error-base); }
		&.critical { color: var(--color--error-base); }
	}
}
</style>
