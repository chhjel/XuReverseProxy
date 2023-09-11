<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import AuditLogService from "@services/AuditLogService";
import { PaginatedResult } from "@generated/Models/Web/PaginatedResult";
import { AdminAuditLogEntry } from "@generated/Models/Core/AdminAuditLogEntry";
import { GetAdminAuditLogEntriesRequestModel } from "@generated/Models/Web/GetAdminAuditLogEntriesRequestModel";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		LoaderComponent
	}
})
export default class AdminAuditLogPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: AuditLogService = new AuditLogService();
	currentPageData: PaginatedResult<AdminAuditLogEntry> | null = null;
	filter: GetAdminAuditLogEntriesRequestModel = {
		fromUtc: new Date(new Date().setDate(new Date().getDate() - 30)),
		toUtc: new Date(new Date().setDate(new Date().getDate() + 1)),
		pageIndex: 0,
		pageSize: 20,
		adminUserId: null,
		proxyConfigId: null,
		clientId: null
	};
	
    proxyConfigService: ProxyConfigService = new ProxyConfigService();
	proxyConfigs: Array<ProxyConfig> = [];

	async mounted() {
		await this.loadReferencedData();
		await this.loadData();
	}

	async loadReferencedData() {
		this.proxyConfigs = (await this.proxyConfigService.GetAllAsync()).data || [];
	}

	async loadData() {
		this.currentPageData = await this.service.GetAdminLogAsync(this.filter);
	}

	get currentPageItems(): Array<AdminAuditLogEntry> {
		if (this.currentPageData == null) return [];
		else return this.currentPageData.pageItems;
	}

	get pageCount(): number {
		const count = Math.ceil((this.currentPageData?.totalItemCount || 0) / this.filter.pageSize);
		return Math.max(1, count);
	}

	formatDate(raw: Date | string): string {
		if (raw == null) return '';
		let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
		return date.toLocaleString();
	}

	async setPage(num: number) {
		this.filter.pageIndex = num - 1;
		await this.loadData();
	}

	createActionHtml(entry: AdminAuditLogEntry): string {
		let html = this.htmlEncode(entry.action);

		if (html.includes('[PROXYCONFIG]')) {
			const existing = this.proxyConfigs.find(x => x.id == entry.relatedProxyConfigId);
			const name = !existing ? 'deleted config'
			 	: this.htmlEncode(existing.name || entry.relatedProxyConfigName) || 'config';
			const linkClass = !existing ? 'missing' : '';
			html = html.replace('[PROXYCONFIG]', `<a href="/#/proxyconfigs/${entry.relatedProxyConfigId}" class="${linkClass}">[${name}]</a>`);
		}

		return html;
	}
	
	htmlEncode(input: string): string {
		if (!input) return input;
		else return input
			.replace(/&/g, '&amp;')
			.replace(/</g, '&lt;')
			.replace(/>/g, '&gt;');
	}
}
</script>

<template>
	<div class="adminauditlog-page">
		<loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce" />
		<div v-if="service.status.hasDoneAtLeastOnce">

			<div class="pagination mb-2">
				<span v-for="number in pageCount" :key="number" style="padding: 5px">
					<a v-if="filter.pageIndex != number - 1" @click.stop.prevent="setPage(number)" href="#">[{{ number }}]</a>
					<span v-if="filter.pageIndex == number - 1">[{{ number }}]</span>
				</span>
			</div>

			<div class="table-wrapper">
				<table>
					<tr>
						<th>When</th>
						<th>Who</th>
						<th>What</th>
					</tr>
					<tr v-for="item in currentPageItems" :key="item.id" class="item">
						<td class="item__when">
							<code :title="formatDate(item.timestampUtc)">{{ formatDate(item.timestampUtc) }}</code>
						</td>
						<td class="item__who">
							<code :title="item.adminUserId">Admin ({{ item.adminUserId }})</code>
						</td>
						<td class="item__what">
							<div v-html="createActionHtml(item)"></div>
						</td>
					</tr>
				</table>
			</div>

			<loader-component :status="service.status" />
		</div>
	</div>
</template>

<style scoped lang="scss">
.adminauditlog-page {
	padding-top: 20px;

	.table-wrapper {
		overflow-x: auto;
	}

    table {
        text-align: left;
		width: 100%;
    }
    th {
        padding: 3px;
    }
    td {
        color: var(--color--text-dark);
        padding: 3px;
    }
    tr {
        border-bottom: 1px solid var(--color--text-darker);
        padding: 3px;
		
        &:nth-child(odd) {
            background-color: var(--color--table-odd);
        }

		&:not(:first-child) {
			/* cursor: pointer; */
			&:hover {
				background-color: var(--color--secondary-darken);
			}
		}
    }

	.item {
		&__when {
			width: 160px;
			max-width: 160px;
			overflow: hidden;
			text-overflow: ellipsis;
			padding: 5px;
		}

		&__who {
			width: 90px;
			max-width: 90px;
			overflow: hidden;
			text-overflow: ellipsis;
		}
		&__what {
			overflow: hidden;
			text-overflow: ellipsis;
		}

		:deep(a.missing) {
			color: var(--color--danger-lighten);
		}
	}
}
</style>
