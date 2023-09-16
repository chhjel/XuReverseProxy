<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
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

	async mounted() {
		this.entries = await this.service.GetLogAsync();
		this.memoryLoggingEnabled = await new ServerConfigService().IsConfigFlagEnabledAsync("EnableMemoryLogging");
	}
	
	formatDate(raw: Date | string): string {
		return DateFormats.defaultDateTime(raw);
	}
}
</script>

<template>
	<div class="log-page">
		<p v-if="memoryLoggingEnabled === false">- Memory logging is currently disabled -</p>
		<div v-for="entry in entries">
			<code>[{{ formatDate(entry.timestampUtc) }}] [{{ entry.logLevel }}] {{ entry.message }}</code>
			<code v-if="entry.exception">{{ entry.exception }}</code>
		</div>
	</div>
</template>

<style scoped lang="scss">
.log-page {
	padding-top: 20px;
}
</style>