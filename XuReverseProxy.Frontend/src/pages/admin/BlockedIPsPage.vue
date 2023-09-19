<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import DateFormats from "@utils/DateFormats";
import { SortByThenBy } from "@utils/SortUtils";
import IPBlockService from "@services/IPBlockService";
import { BlockedIpData } from "@generated/Models/Core/BlockedIpData";
import { BlockedIpDataType } from "@generated/Enums/Core/BlockedIpDataType";
import { EmptyGuid } from "@utils/Constants";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		LoaderComponent
	}
})
export default class BlockedIPsPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
	service: IPBlockService = new IPBlockService();
	rules: Array<BlockedIpData> = [];

	async mounted() {
		const result = await this.service.GetAllAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.rules = result.data || [];
	}

	get sortedRules(): Array<BlockedIpData> {
		return this.rules.sort((a,b) => SortByThenBy(a, b, 
			x => x.enabled, x => x.name,
			(a, b) => <any>b.enabled - <any>a.enabled,
			(a, b) => a.name?.localeCompare(b.name)
		));
	}

	getRuleStatus(rule: BlockedIpData): string {
		if (!rule.enabled) return '(disabled)';
		else if (rule.blockedUntilUtc && new Date(rule.blockedUntilUtc).getTime() < new Date().getTime()) return '(expired)';
		else '';
	}

	getRuleIcon(rule: BlockedIpData): any {
		if (!rule.enabled) return 'do_not_disturb_on';
		else if (rule.blockedUntilUtc && new Date(rule.blockedUntilUtc).getTime() < new Date().getTime()) return 'hourglass_empty';
		else return 'network_ping';
	}

	getRuleIconClasses(rule: BlockedIpData): any {
		let classes: any = {};
		if (!rule.enabled) classes['disabled'] = true;
		else if (rule.blockedUntilUtc && new Date(rule.blockedUntilUtc).getTime() < new Date().getTime()) classes['disabled'] = true;
		return classes;
	}

	async addNewRule() {
		let rule: BlockedIpData = {
			id: EmptyGuid,
			enabled: false,
			name: 'New IP block rule',
			note: '',
			blockedAt: new Date(),
			blockedUntilUtc: null,
			type: BlockedIpDataType.IP,
			ip: '',
			cidrRange: '',
			ipRegex: '',
			relatedClientId: null
		};
		const result = await this.service.CreateOrUpdateAsync(rule);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			rule = result.data;
			this.rules.push(rule);
			this.$router.push({ name: 'blocked-ip', params: { blockedId: rule.id } });
		}
	}

	createSummary(rule: BlockedIpData): string {
		if (rule.type == BlockedIpDataType.IP) return  `Block single ip '${rule.ip}'`;
		else if (rule.type == BlockedIpDataType.IPRegex) return  `Block RegEx '${rule.ipRegex}'`;
		else if (rule.type == BlockedIpDataType.CIDRRange) return  `Block CIDR range '${rule.cidrRange}'`;
		else return '';
	}

	formatDate(raw: Date | string): string {
		return DateFormats.defaultDateTime(raw);
	}
}
</script>

<template>
	<div class="blocked-ips-page">
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
			<div v-if="sortedRules.length == 0 && service.status.done">- No rules configured yet -</div>
			<div v-for="rule in sortedRules" :key="rule.id">
				<router-link :to="{ name: 'blocked-ip', params: { blockedId: rule.id }}" class="rule">
					<div class="rule__header">
						<div class="material-icons icon" :class="getRuleIconClasses(rule)">{{ getRuleIcon(rule) }}</div>
						<div class="rule__name">{{ rule.name }} <span class="rule__status">{{ getRuleStatus(rule) }}</span></div>
					</div>
					<div class="rule__summary">
						<code>{{ createSummary(rule) }}</code>
					</div>
				</router-link>
			</div>
			<button-component @click="addNewRule" v-if="service.status.done"
				class="primary ml-0">Add new IP block rule</button-component>
		</div>
	</div>
</template>

<style scoped lang="scss">
.blocked-ips-page {
	padding-top: 20px;
	
	.rule {
		display: block;
		padding: 5px;
		margin: 5px 0;

		&:hover {
			text-decoration: none;
			background-color: var(--color--hover-bg);
		}

		&__header {
			display: flex;
			align-items: center;
		}

		&__status {
			font-size: 12px;
			color: var(--color--text-darker);
		}

		&__summary {
			display: flex;
			align-items: center;
			flex-wrap: wrap;
			font-size: 12px;
			color: var(--color--secondary);
			margin-left: 31px;
		}

		&__lastresult {
			color: var(--color--secondary);
			font-size: 12px;
			margin-left: 31px;
		}

		.icon {
			width: 24px;
			margin-right: 5px;
			color: var(--color--primary-lighten);

			&.disabled {
				color: var(--color--warning-base);
			}
		}
	}
}
</style>
