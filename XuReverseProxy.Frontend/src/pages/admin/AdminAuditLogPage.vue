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
		pageSize: 5,
		adminUserId: null,
		proxyConfigId: null,
		clientId: null
	};

	async mounted() {
		await this.loadData();
	}

	async loadData() {
		this.currentPageData = await this.service.GetAdminLogAsync(this.filter);
	}

	get currentPageItems(): Array<AdminAuditLogEntry> {
		if (this.currentPageData == null) return [];
		else return this.currentPageData.pageItems;
	}

	formatDate(raw: Date | string): string {
		if (raw == null) return '';
		let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
		return date.toLocaleString();
	}
}
</script>

<template>
	<div class="proxyclients-page">
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
			<code>{{ filter }}</code>
			<hr>
			
			<!--
			/*
			* 3 cols table in frontend:
				When: Timestamp
				Who: [admin] / [1.2.3.4:3d3a5e91]
				What: e.g:
					Admin:
						Created new proxy config [Test 3 config]
													^ links to related item, use resolved name. If deleted, color red and use name from audit item.
						Manually approved client [1.2.3.4:3d3a5e91] for proxy config [Test 3 config].
						Blocked client [1.2.3.4:3d3a5e91].
						Blocked IP 1.2.3.4.
					Client:
						Completed login challenge for [Test 3 config]
			*/ -->

			<div class="table-wrapper">
				<table>
					<tr>
						<th>When</th>
						<th>Who</th>
						<th>What</th>
					</tr>
					<tr v-for="item in currentPageItems" :key="item.id" class="item">
						<td class="item__when">
							<code>{{ formatDate(item.timestampUtc) }}</code>
						</td>
						<td class="item__when">
							<code>Admin ({{ item.adminUserId }})</code>
						</td>
						<td class="client__what">
							<code>{{ item }}</code>
						</td>
					</tr>
				</table>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyclients-page {
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
			cursor: pointer;
			&:hover {
				background-color: var(--color--secondary);
			}
		}
    }

	.client {
		&__id {
			max-width: 70px;
			overflow: hidden;
			text-overflow: ellipsis;
		}

		&__useragent {
			max-width: 177px;
			overflow: hidden;
			text-overflow: ellipsis;
		}
		&__note {
			max-width: 300px;
			overflow: hidden;
			text-overflow: ellipsis;
		}
	}
}
</style>
