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
import DateFormats from "@utils/DateFormats";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		LoaderComponent
	}
})
export default class ProxyClientsPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ProxyClientIdentityService = new ProxyClientIdentityService();
	clients: Array<ProxyClientIdentity> = [];

	async mounted() {
		const result = await this.service.GetAllAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.clients = result.data || [];
	}

	get pagedClients(): Array<ProxyClientIdentity> {
		return this.clients;
	}

	navToClient(clientId: string, newTab: boolean = false): void {
		if (newTab) window.open(`/#/client/${clientId}`, '_blank');
		else this.$router.push({ name: 'client', params: { clientId: clientId } });
	}

	formatDate(raw: Date | string): string {
		return DateFormats.defaultDateTime(raw);
	}
}
</script>

<template>
	<div class="proxyclients-page">
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
			<ul>
				Todo
				<li>Get paged</li>
				<li>Delete single/all/not used in a month/week/year</li>
			</ul>

			<p>Note: Clients are only created for proxies with authentication.</p>

			<div class="table-wrapper">
				<table>
					<tr>
						<th>Note</th>
						<th>UserAgent</th>
						<th>IP</th>
						<th>Id</th>
						<th>Last access</th>
						<th style="font-size: 12px;">Last attempted access</th>
						<th></th>
					</tr>
					<tr v-for="client in pagedClients" :key="client.id" class="client"
						@click="navToClient(client.id)"
						@click.middle="navToClient(client.id, true)">
						<td class="client__note">
							<code :title="client.note">{{ client.note }}</code>
						</td>
						<td class="client__useragent">
							<code :title="client.userAgent">{{ client.userAgent }}</code>
						</td>
						<td class="client__ip">
							<code :title="client.ip">{{ client.ip }}</code>
						</td>
						<td class="client__id">
							<code :title="client.id">{{ client.id }}</code>
						</td>
						<td class="client__la">
							<code v-if="client.lastAccessedAtUtc">{{ formatDate(client.lastAccessedAtUtc) }}</code>
						</td>
						<td class="client__laa">
							<code v-if="client.lastAttemptedAccessedAtUtc">{{ formatDate(client.lastAttemptedAccessedAtUtc) }}</code>
						</td>
						<td class="client__meta">
							<code v-if="client.blocked">Blocked</code>
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
