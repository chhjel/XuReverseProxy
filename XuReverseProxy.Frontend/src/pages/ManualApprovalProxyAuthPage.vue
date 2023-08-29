<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ManualApprovalProxyAuthPageFrontendModel } from "@generated/Models/Web/ManualApprovalProxyAuthPageFrontendModel";
import ManualApprovalService from "@services/ManualApprovalService";
import MapComponent from "@components/common/MapComponent.vue";
import { ChallengeDataFrontendModel } from "@generated/Models/Web/ChallengeDataFrontendModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		MapComponent
	}
})
export default class ManualApprovalProxyAuthPage extends Vue {
  	@Prop()
	options: ManualApprovalProxyAuthPageFrontendModel;
	
    service: ManualApprovalService | null= null;
	isApproved: boolean = false;
	isBlocked: boolean = false;
	clientNote: string = '';

	async mounted() {
		this.service = new ManualApprovalService(this.options.client.id, this.options.authenticationId, this.options.solvedId);
		this.isBlocked = this.options.client.blocked;
		this.isApproved = this.options.isApproved;
		this.clientNote = this.options.client.note || '';
	}

	get isLoading(): boolean { return this.service?.status?.inProgress == true; }
	
	async onApproveClicked(): Promise<any> { await this.approve(); }
	async onUnApproveClicked(): Promise<any> { await this.unApprove();}
	async onBlockClicked(): Promise<any> { await this.setClientBlocked(true, 'Blocked!')};
	async onUnBlockClicked(): Promise<any> { await this.setClientBlocked(false, '')};
	async updateClientNote(): Promise<any> { await this.setClientNote(this.clientNote)};

	async approve(): Promise<any> {
		const success = await this.service.ApproveAsync();
		if (success) this.isApproved = true;
	}
	async unApprove(): Promise<any> {
		const success = await this.service.UnapproveAsync();
		if (success) this.isApproved = false;
	}
	async setClientBlocked(blocked: boolean, message: string): Promise<any> {
		const success = await this.service.SetClientBlockedAsync({
			blocked: blocked,
			message: message
		});
		if (success) this.isBlocked = blocked;
	}
	async setClientNote(note: string): Promise<any> {
		await this.service.SetClientNoteAsync({
			note: note
		});
	}

	get ipContinent(): string | null { return this.options.client.ipLocation?.continent || null; }
	get ipCountry(): string | null { return this.options.client.ipLocation?.country || null; }
	get ipCity(): string | null { return this.options.client.ipLocation?.city || null; }
	get ipFlagUrl(): string | null { return this.options.client.ipLocation?.flagUrl || null; }

	formatDate(raw: Date | string): string {
		if (raw == null) return '';
		let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
		return date.toLocaleString();
	}

	getChallengeTypeIdName(typeId: string): string {
		return typeId.replace('ProxyChallengeType', '');
	}

	getChallengeIcon(challenge: ChallengeDataFrontendModel): string {
		if (challenge.solved) return 'done';
		else if (challenge.conditionsNotMet) return 'schedule';
		else return 'close';
	}

	getChallengeClasses(challenge: ChallengeDataFrontendModel): any {
		return {
			'solved': challenge.solved || challenge.conditionsNotMet
		};
	}

	getChallengeTooltip(challenge: ChallengeDataFrontendModel): any {
		if (challenge.solved) return `Challenge passed at ${this.formatDate(challenge.solvedAtUtc)}`;
		else if (challenge.conditionsNotMet) return 'Challenge conditions not met';
		else return 'Challenge not passed yet';
	}
}
</script>

<template>
	<div class="manual-approval-page">
		
		<button-component v-if="!isApproved" @click="onApproveClicked" :disabled="isLoading" class="ml-0">Approve</button-component>
		<button-component v-if="isApproved" @click="onUnApproveClicked" :disabled="isLoading" class="ml-0">Remove approval</button-component>
		<button-component v-if="!isBlocked" @click="onBlockClicked" :disabled="isLoading" class="ml-0">Block client</button-component>
		<button-component v-if="isBlocked" @click="onUnBlockClicked" :disabled="isLoading" class="ml-0">Unblock client</button-component>
		<hr>
		<textarea v-model="clientNote" @blur="updateClientNote"></textarea>
		<hr>
		<div>Approved: {{ isApproved }}</div>
		<div>Blocked: {{ isBlocked }}</div>
		<hr>

		<p>
			<b>{{ options.client.ip }}</b>/<b>{{options.client.userAgent}}</b> requests access to 
			<b>{{ options.proxyConfig.name }}</b> <i>({{ options.proxyConfig.challengeTitle }})</i> at 
			<a :href="options.url">{{ options.url }}</a>
		</p>

		<!-- todo: padlock animation that opens on approve? -->

		<div>Code: {{ options.currentChallengeData.easyCode }}</div>
		<div>Requested at: {{ formatDate(options.currentChallengeData.requestedAt) }}</div>
		<div>UserAgent: {{ options.client.userAgent }}</div>
		<div v-if="options.client.createdAtUtc">Client first known at: {{ formatDate(options.client.createdAtUtc) }}</div>
		<div v-if="options.client.lastAttemptedAccessedAtUtc">Last attempted accessed at: {{ formatDate(options.client.lastAttemptedAccessedAtUtc) }}</div>
		<div v-if="options.client.lastAccessedAtUtc">Last accessed at: {{ formatDate(options.client.lastAccessedAtUtc) }}</div>
		
		<div class="ipdetails" v-if="options.client.ipLocation?.success == true">

			<div class="ipdetails-location">
				<div v-if="ipContinent" class="ipdetails-location__part">{{ ipContinent }}</div>
				<div v-if="ipFlagUrl || ipCountry" class="ipdetails-location__part">
					<img :src="ipFlagUrl" class="ipdetails-flag" style="max-height: 16px; position: relative; top: 2px;" />
					<span v-if="ipCountry">{{ ipCountry }}</span>
				</div>
				<div v-if="ipCity">{{ ipCity }}</div>
			</div>

			<map-component class="map"
				v-if="options.client.ipLocation.latitude && options.client.ipLocation.longitude"
				:lat="options.client.ipLocation.latitude"
				:lon="options.client.ipLocation.longitude"
				:zoom="12"
				note="Client location"
				/>
		</div>

		<div class="challenges mt-4" v-if="options.allChallengeData.length > 0">
			<div class="challenges__title">Challenge statuses</div>
			<div v-for="challenge in options.allChallengeData" class="challenge" :title="getChallengeTooltip(challenge)">
				<div class="material-icons icon" :class="getChallengeClasses(challenge)">{{ getChallengeIcon(challenge) }}</div>
				<div>{{ getChallengeTypeIdName(challenge.type) }}</div>
			</div>
		</div>

		<!-- <hr class="mt-4 mb-4">
		<code>{{ options }}</code> -->
	</div>
</template>

<style scoped lang="scss">
.manual-approval-page {
    max-width: 800px;
    margin: auto;
	padding: 40px;
	@media (max-width: 800px) {
		padding: 10px;
	}

	.details-span {
		margin-right: 5px;
	}

	.challenge {
		display: flex;
		align-items: center;
	}

	.icon {
		color: var(--color--warning-base);
		&.solved {
			color: var(--color--success-base);
		}
	}
}
</style>
