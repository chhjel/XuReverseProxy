<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";
import StringUtils from "@utils/StringUtils";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import MapComponent from "@components/common/MapComponent.vue";
import { IPLookupResult } from "@generated/Models/Core/IPLookupResult";
import IPLookupService from "@services/IPLookupService";
import IPBlockService from "@services/IPBlockService";
import { LoadStatus } from "@services/ServiceBase";
import ProxyAuthenticationDataService from "@services/ProxyAuthenticationDataService";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import { GenericResult } from "@generated/Models/Web/GenericResult";
import GlobeComponent from "@components/common/GlobeComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		CheckboxComponent,
		MapComponent,
		GlobeComponent,
		LoaderComponent
	}
})
export default class ProxyClientPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ProxyClientIdentityService = new ProxyClientIdentityService();
    proxyConfigService: ProxyConfigService = new ProxyConfigService();
    proxyAuthService: ProxyAuthenticationDataService = new ProxyAuthenticationDataService();
    // proxyAuthConditionService: ProxyAuthenticationConditionService = new ProxyAuthenticationConditionService();
	statuses: Array<LoadStatus> = [this.service.status, this.proxyConfigService.status, this.proxyAuthService.status];
	client: ProxyClientIdentity | null = null;
	clientId: string = '';

	ipLookupService: IPLookupService = new IPLookupService();
	ipLookupData: IPLookupResult | null = null;
	ipBlockService: IPBlockService = new IPBlockService();
	
	isBlocked: boolean = false;
	clientBlockedNote: string = '';
	isIpBlocked: boolean = false;
	blockedIpId: string | null = null;
	canUnblockIp: boolean = false;
	clientNote: string = '';

	proxyConfigs: Array<ProxyConfig> = [];
	proxyConfigAuths: Array<ProxyAuthenticationData> = [];

	clientAuthData: Array<any> = [];

	async mounted() {
		this.clientId = StringUtils.firstOrDefault(this.$route.params.clientId);
		const result = await this.service.GetAsync(this.clientId);
		if (!result.success) {
			console.error(result.message);
		} else {
			this.client = result.data;
			this.onClientLoaded();
		}
	}

	async onClientLoaded(): Promise<any> {
		this.isBlocked = this.client.blocked;
		this.clientBlockedNote = this.client.blockedMessage;
		this.clientNote = this.client.note;

		this.ipLookupService.LookupIPAsync(this.client.ip).then(x => this.ipLookupData = x);
		
		this.ipBlockService.GetMatchingBlockedIpDataForAsync(this.client.ip).then(x => {
			this.isIpBlocked = x != null;
			this.blockedIpId = x?.id;
			this.canUnblockIp = x != null && x.ip != null && x.ip.length > 0;
		});

		this.proxyConfigs = (await this.proxyConfigService.GetAllAsync())?.data || [];
		this.proxyConfigAuths = (await this.proxyAuthService.GetAllAsync())?.data || [];
		this.client.solvedChallenges.forEach(x => {
			const auth = this.proxyConfigAuths.find(a => a.id == x.authenticationId);
			const proxyConfig = this.proxyConfigs.find(c => c.id == auth?.proxyConfigId);
			if (auth && proxyConfig) {
				this.clientAuthData.push({
					proxyConfig: proxyConfig,
					auth: auth,
					solvedData: x
				});
			}
		});
	}

	get isLoading(): boolean { return this.statuses.some(x => x.inProgress); }
	
	get ipContinent(): string | null { return this.ipLookupData?.continent || null; }
	get ipCountry(): string | null { return this.ipLookupData?.country || null; }
	get ipCity(): string | null { return this.ipLookupData?.city || null; }
	get ipFlagUrl(): string | null { return this.ipLookupData?.flagUrl || null; }

	async updateClientNote(): Promise<any> {
		await this.service.SetClientNoteAsync({
			clientId: this.client.id,
			note: this.clientNote
		});
	}

	async toggleClientBlocked(): Promise<any> {
		const oldValue = this.isBlocked;
		this.isBlocked = !this.isBlocked;
		const result = await this.updateClientBlocked()
		if (result?.success !== true) this.isBlocked = oldValue;
	}

	async updateClientBlocked(): Promise<GenericResult> {
		return await this.service.SetClientBlockedAsync({
			clientId: this.client.id,
			message: this.clientBlockedNote,
			blocked: this.isBlocked
		});
	}
}
</script>

<template>
	<div class="proxyclient-page">
		<loader-component :status="statuses" v-if="!service.status.hasDoneAtLeastOnce" />

		<div v-if="client">
			<!-- Metadata -->
			<div class="block overflow-x-scroll mb-4 pt-2">
				<div><code>IP = {{ client.ip }}</code></div>
				<div><code>UserAgent = {{ client.userAgent }}</code></div>
				<div><code>CreatedAtUtc = {{ client.createdAtUtc }}</code></div>
				<div><code>LastAccessedAtUtc = {{ client.lastAccessedAtUtc }}</code></div>
				<div><code>LastAttemptedAccessedAtUtc = {{ client.lastAttemptedAccessedAtUtc }}</code></div>
			</div>

			<!-- Notes -->
			<div class="block mb-4 pt-2">
				<div class="block-title">Client note</div>
				<div class="input-wrapper">
					<textarea id="clientNote" v-model="clientNote" @blur="updateClientNote"></textarea>
				</div>
			</div>
			
			<!-- Blocked -->
			<div class="block mb-4 pt-2">
				<div class="block-title">Client block</div>
				<checkbox-component label="Blocked" offLabel="Not blocked"
						:value="isBlocked" class="mt-1 mb-2" warnColorOn
						@click="toggleClientBlocked"
						:disabled="isLoading" />
				<div><code>blockedAtUtc = {{ (client.blockedAtUtc || 'null')}}</code></div>
				<div class="input-wrapper">
					<textarea id="clientBlockedNote" v-model="clientBlockedNote" @blur="updateClientBlocked" placeholder="Blocked message"></textarea>
				</div>
			</div>

			<!-- IP block -->
			<div class="block overflow-x-scroll mb-4 pt-2">
				<div class="block-title">IP block</div>
				<div><code>isIpBlocked = {{ isIpBlocked }}</code></div>
				<div><code>blockedIpId = {{ (blockedIpId || 'null') }}</code></div>
				<div><code>canUnblockIp = {{ canUnblockIp }}</code></div>
			</div>

			<!-- IP LOCATION -->
			<div class="block mb-4" v-if="ipLookupData?.success == true">
				<div class="ipdetails-location">
					<div v-if="ipContinent" class="ipdetails-location__part">{{ ipContinent }}</div>
					<div v-if="ipFlagUrl || ipCountry" class="ipdetails-location__part flag-part">
						<img :src="ipFlagUrl" class="ipdetails-flag" />
						<span v-if="ipCountry">{{ ipCountry }}</span>
					</div>
					<div v-if="ipCity" class="ipdetails-location__part">{{ ipCity }}</div>
				</div>
			</div>

			<!-- MAP -->
			<div class="block mb-4" v-if="ipLookupData?.success == true && ipLookupData.latitude && ipLookupData.longitude">
				<map-component class="map" :lat="ipLookupData.latitude" :lon="ipLookupData.longitude"
					:zoom="12" note="Client location" />
			</div>

			<!-- GLOBE -->
			<div class="block no-bg mb-4 pa-0" v-if="ipLookupData?.success == true && ipLookupData.latitude && ipLookupData.longitude">
				<globe-component class="globe" :lat="ipLookupData.latitude" :lon="ipLookupData.longitude" :ping="true" />
			</div>

			<!-- Solved data -->
			<div class="block overflow-x-scroll mb-4 pt-2">
				<div class="block-title">Solved client challenges</div>
				<code>{{ clientAuthData }}</code>
			</div>
		</div>

		<hr>
		<div v-if="client">
			<div class="block overflow-x-scroll mb-4 pt-2">
				<code>{{ client }}</code>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyclient-page {
	padding-top: 20px;

	.ipdetails-location {
		display: flex;
		flex-wrap: wrap;

		.ipdetails-flag {
			position: relative;
			top: 1px;
			width: 22px;
			max-height: 18px;
			margin-right: 3px;
		}

		.ipdetails-location__part {
			white-space: nowrap;

			&.flag-part {
				display: flex;
			}

			&:not(:last-child) {
				&::after {
					content: '>';
					font-size: 15px;
					display: inline-block;
					margin: 0 5px;
				}
			}
		}
	}

	.globe {
		height: 300px;
	}
}
</style>
