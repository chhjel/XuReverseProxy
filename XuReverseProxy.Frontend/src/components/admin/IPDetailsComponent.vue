<script lang="ts">
import GlobeComponent from "@components/common/GlobeComponent.vue";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import MapComponent from "@components/common/MapComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import { IPLookupResult } from "@generated/Models/Core/IPLookupResult";
import { GenericResult } from "@generated/Models/Web/GenericResult";
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
		CheckboxComponent,
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
    
	async toggleIPBlocked(): Promise<any> {
        this.blockedIpNote = this.blockedIpNote || '';

		const oldValue = this.isIpBlocked;
		this.isIpBlocked = !this.isIpBlocked;
		const success = await this.updateIPBlocked()
		if (!success) this.isIpBlocked = oldValue;
	}

	async updateIPBlocked(): Promise<boolean> {
        if (this.isIpBlocked) return await this.blockIP()
        else return await this.unblockIP();
	}

    async blockIP(): Promise<boolean> {
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
            return true;
        }
        return false;
    }

    async unblockIP(): Promise<boolean> {
        if (!this.blockedIpId) return;
        const result = await this.ipBlockService.RemoveIPBlockByIdAsync({
            id: this.blockedIpId
        });
        if (result.success) {
            this.blockedIpId = null;
            this.blockedIpNote = null;
            this.isIpBlocked = false;
			this.canUnblockIp = false;
            return true;
        }
        return false;
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

            <div v-if="isLocalhost" class="block mb-4">
				<div class="block-title">IP details</div>
                <p>Can't show any more details for localhost.</p>
            </div>

            <div v-if="isValidIp">
                <!-- IP LOCATION -->
                <div class="block mb-4">
				    <div class="block-title">{{ ip }}</div>
                    <div v-if="ipLookupData?.success == true">
                        <div class="ipdetails-location mt-3">
                            <div v-if="ipContinent" class="ipdetails-location__part">{{ ipContinent }}</div>
                            <div v-if="ipFlagUrl || ipCountry" class="ipdetails-location__part flag-part">
                                <img :src="ipFlagUrl" class="ipdetails-flag" />
                                <span v-if="ipCountry">{{ ipCountry }}</span>
                            </div>
                            <div v-if="ipCity" class="ipdetails-location__part">{{ ipCity }}</div>
                        </div>
                    </div>

                    <div v-if="canUnblockIp || !isIpBlocked">
                        <checkbox-component label="IP blocked" offLabel="IP not blocked"
                                :value="isIpBlocked" class="mt-3" warnColorOn
                                @click="toggleIPBlocked"
                                :disabled="isLoading" />
                        <div v-if="blockedIpNote" class="mt-2"><code>{{ blockedIpNote}}</code></div>
                    </div>
                    <div v-if="!canUnblockIp && isIpBlocked" class="mt-4">
                        IP is currently blocked indirectly through a RegEx or CIDR rule.
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
