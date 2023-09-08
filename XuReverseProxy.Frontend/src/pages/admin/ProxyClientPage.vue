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

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		MapComponent,
		LoaderComponent
	}
})
export default class ProxyClientPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ProxyClientIdentityService = new ProxyClientIdentityService();
	client: ProxyClientIdentity | null = null;
	clientId: string = '';

	ipLookupService: IPLookupService = new IPLookupService();
	ipLookupData: IPLookupResult | null = null;

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

	onClientLoaded(): void {
		this.ipLookupService.LookupIPAsync(this.client.ip).then(x => this.ipLookupData = x);
	}
	
	get ipContinent(): string | null { return this.ipLookupData?.continent || null; }
	get ipCountry(): string | null { return this.ipLookupData?.country || null; }
	get ipCity(): string | null { return this.ipLookupData?.city || null; }
	get ipFlagUrl(): string | null { return this.ipLookupData?.flagUrl || null; }
}
</script>

<template>
	<div class="proxyclient-page">
		<loader-component :status="service.status" />

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

		<hr>
		<div v-if="client">
			<code>{{ client }}</code>
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
}
</style>
