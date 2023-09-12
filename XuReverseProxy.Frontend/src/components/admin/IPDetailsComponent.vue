<script lang="ts">
import GlobeComponent from "@components/common/GlobeComponent.vue";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import MapComponent from "@components/common/MapComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { IPLookupResult } from "@generated/Models/Core/IPLookupResult";
import IPBlockService from "@services/IPBlockService";
import IPLookupService from "@services/IPLookupService";
import { LoadStatus } from "@services/ServiceBase";
import { Options } from "vue-class-component";
import { Prop, Vue } from 'vue-property-decorator'

@Options({
	components: {
        ButtonComponent,
		LoaderComponent,
		MapComponent,
		GlobeComponent,
	}
})
export default class IPDetailsComponent extends Vue {
    @Prop({ required: true })
    ip!: string;

    @Prop({ required: false, default: null })
    relatedClientId!: string | null;
    
	ipLookupService: IPLookupService = new IPLookupService();
	ipLookupData: IPLookupResult | null = null;
	ipBlockService: IPBlockService = new IPBlockService();
	isIpBlocked: boolean = false;
	blockedIpId: string | null = null;
	blockedIpNote: string | null = null;
	canUnblockIp: boolean = false;
	statuses: Array<LoadStatus> = [this.ipLookupService.status, this.ipBlockService.status];

	async mounted() {
        this.ipLookupService.LookupIPAsync(this.ip).then(x => {
            this.ipLookupData = x;
        });
        
        this.ipBlockService.GetMatchingBlockedIpDataForAsync(this.ip).then(x => {
            this.isIpBlocked = x != null;
			this.blockedIpId = x?.id;
            this.blockedIpNote = x?.note;
			this.canUnblockIp = x != null && x.ip != null && x.ip.length > 0;
        });
	}

    async blockIP() {
        const note = prompt('Note (optional)');
        if (note === null) return;
        const result = await this.ipBlockService.BlockIPAsync({
            ip: this.ip,
            note: note || '',
            relatedClientId: this.relatedClientId
        });
        if (result) {
            this.blockedIpId = result.id;
            this.blockedIpNote = result.note;
            this.isIpBlocked = true;
			this.canUnblockIp = true;
        }
    }

    async unblockIP() {
        if (!this.blockedIpId) return;
        const result = await this.ipBlockService.RemoveIPBlockByIdAsync({
            id: this.blockedIpId
        });
        if (result.success) {
            this.blockedIpId = null;
            this.blockedIpNote = null;
            this.isIpBlocked = false;
			this.canUnblockIp = false;
        }
    }

	get isLoading(): boolean { return this.statuses.some(x => x.inProgress); }

	get isLocalhost(): boolean { return this.ip === 'localhost'; }
	get isValidIp(): boolean { return this.ip && !this.isLocalhost; }
	get ipContinent(): string | null { return this.ipLookupData?.continent || null; }
	get ipCountry(): string | null { return this.ipLookupData?.country || null; }
	get ipCity(): string | null { return this.ipLookupData?.city || null; }
	get ipFlagUrl(): string | null { return this.ipLookupData?.flagUrl || null; }
}
</script>

<template>
	<nav class="ip-details">
		<loader-component :status="statuses" v-if="!ipLookupService.status.hasDoneAtLeastOnce" />

        <div v-if="ipLookupService.status.hasDoneAtLeastOnce">
            <div v-if="ipLookupData == null">
                <h2>IP location was not found.</h2>
            </div>

            <div v-if="isLocalhost">
                <p>Can't show any more details for localhost.</p>
            </div>

            <div v-if="isValidIp">
                <!-- IP LOCATION -->
                <div class="block mb-4" v-if="ipLookupData?.success == true">
                    <h2 class="mt-0">{{ ip }}</h2>
                    <div class="ipdetails-location">
                        <div v-if="ipContinent" class="ipdetails-location__part">{{ ipContinent }}</div>
                        <div v-if="ipFlagUrl || ipCountry" class="ipdetails-location__part flag-part">
                            <img :src="ipFlagUrl" class="ipdetails-flag" />
                            <span v-if="ipCountry">{{ ipCountry }}</span>
                        </div>
                        <div v-if="ipCity" class="ipdetails-location__part">{{ ipCity }}</div>
                    </div>
                    
                    <!-- IP block -->
                    <p v-if="isIpBlocked" class="mb-2 mt-4">IP is currently blocked.</p>

                    <div v-if="blockedIpNote">Note: <code>{{ blockedIpNote }}</code></div>
                    <p v-if="isIpBlocked && !canUnblockIp">IP has been blocked indirectly through a regex or CIDR range.</p>

                    <div class="mt-2">
                        <button-component @click="blockIP" class="ml-0" danger v-if="!isIpBlocked">Block IP</button-component>
                        <button-component @click="unblockIP" class="ml-0" v-if="canUnblockIp">Unblock IP</button-component>
                    </div>
                </div>
                
                <!-- MAP -->
                <div class="block mb-4 pa-0" v-if="ipLookupData?.success == true && ipLookupData.latitude && ipLookupData.longitude">
                    <map-component class="map" :lat="ipLookupData.latitude" :lon="ipLookupData.longitude"
                        :zoom="12" note="Client location" />
                </div>

                <!-- GLOBE -->
                <div class="block no-bg mb-4 pa-0" v-if="ipLookupData?.success == true && ipLookupData.latitude && ipLookupData.longitude">
                    <globe-component class="globe" :lat="ipLookupData.latitude" :lon="ipLookupData.longitude" :ping="true" />
                </div>
            </div>
        </div>
	</nav>
</template>

<style scoped lang="scss">
.ip-details {
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
