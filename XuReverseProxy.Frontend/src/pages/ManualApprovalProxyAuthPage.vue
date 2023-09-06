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
	isIpBlocked: boolean = false;
	blockedIpId: string | null = null;
	canUnblockIp: boolean = false;
	clientNote: string = '';

	async mounted() {
		this.service = new ManualApprovalService(this.options.client.id, this.options.authenticationId, this.options.solvedId);
		this.isBlocked = this.options.client.blocked;
		this.isApproved = this.options.isApproved;
		this.isIpBlocked = this.options.clientIPIsBlocked;
		this.canUnblockIp = this.options.canUnblockIP;
		this.blockedIpId = this.options.clientIPBlockId;
		this.clientNote = this.options.client.note || '';
	}

	get isLoading(): boolean { return this.service?.status?.inProgress == true; }
	
	async onApproveClicked(): Promise<any> { await this.approve(); }
	async onUnApproveClicked(): Promise<any> { await this.unApprove(); }
	async onBlockClicked(): Promise<any> { 
		const message = prompt('Blocked message', 'You have been blocked');
		if (message === null) return;
		await this.setClientBlocked(true, message);
	}
	async onUnBlockClicked(): Promise<any> { await this.setClientBlocked(false, ''); }
	async updateClientNote(): Promise<any> { await this.setClientNote(this.clientNote); }
		
	async onBlockIpClicked(): Promise<any> {
		if (this.isLoading) return;

		if (this.options.selfIP == this.options.client.ip && !confirm(`The IP is the same as your own (${this.options.client.ip}), sure you want to proceed? You will loose access to this interface.`)) return;
		
		if (!confirm(`Block IP "${this.options.client.ip}"?`)) return;
		const note = prompt('Note', '');
		if (note === null) return;
		await this.blockClientIp(this.options.client.ip, note);
	}

	async onUnBlockIpClicked(): Promise<any> {
		if (this.isLoading) return;
		if (!confirm(`Unblock IP "${this.options.client.ip}"?`)) return;
		await this.unblockClientIp(this.blockedIpId);
	}

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
	async blockClientIp(ip: string, note: string): Promise<any> {
		this.blockedIpId = await this.service.SetClientIPBlockedAsync({
			ip: ip,
			note: note
		});
		
		this.isIpBlocked = true;
		this.canUnblockIp = true;
	}
	async unblockClientIp(ipBlockId: string): Promise<any> {
		await this.service.RemoveIpBlockAsync({
			ipBlockId: ipBlockId
		});
		this.isIpBlocked = false;
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

	toggleTruncate(e: Event): void {
		const el = e.target as HTMLElement;
		if (el?.classList?.contains('truncate') == true) {
			el?.classList?.remove('truncate');
		} else {
			el?.classList?.add('truncate');
		}
	}

	get formattedEasyCode(): string {
		if (!this.options.currentChallengeData.easyCode || this.options.currentChallengeData.easyCode.length < 6) return this.options.currentChallengeData.easyCode;
		else return `${this.options.currentChallengeData.easyCode.substring(0, 2)} ${this.options.currentChallengeData.easyCode.substring(2, 4)} ${this.options.currentChallengeData.easyCode.substring(4, 6)}`
	}

	get status(): string {
		if (this.isIpBlocked && this.isApproved) return 'Client is approved but its IP is currently blocked';
		else if (this.isIpBlocked) return 'Client IP is currently blocked';
		else if (this.isBlocked && this.isApproved) return 'Client is currently approved and blocked';
		else if (this.isBlocked) return 'Client is currently blocked';
		else if (this.isApproved) return 'Client is currently approved';
		else return 'Client is waiting for approval';
	}

	get statusClass(): any {
		let classes: any = {};
		if (this.isBlocked || this.isIpBlocked) return 'blocked';
		else if (this.isApproved) return 'approved';
		return classes;
	}

	get title(): string {
		return this.options.proxyConfig.challengeTitle || this.options.proxyConfig.name || 'a service';
	}

	get showAka(): boolean {
		return this.options.proxyConfig.challengeTitle
			&& this.options.proxyConfig.challengeTitle != this.options.proxyConfig.name;
	}
}
</script>

<template>
	<div class="manual-approval-page">
		<div class="header">
			<h1 class="title">Client requests access to <div>{{ title }}</div></h1>
			<div class="aka" v-if="showAka">(aka <span>{{ options.proxyConfig.name }}</span>)</div>
			<div class="link">on url <a :href="options.url">{{ options.url }}</a></div>
		</div>
		
		<!-- Reference code -->
		<div class="easycode-wrapper block block--secondary mt-5 mb-4">
			<div class="easycodeTitle">Reference code</div>
			<div class="easycode">{{ formattedEasyCode }}</div>
		</div>

		<div class="status" :class="statusClass">{{ status }}</div>

		<!-- Actions -->
		<div class="actions">
			<button-component v-if="!isApproved" @click="onApproveClicked" :disabled="isLoading" class="ml-0 action">Approve</button-component>
			<button-component v-if="isApproved" @click="onUnApproveClicked" :disabled="isLoading" class="ml-0 action">Remove approval</button-component>
			<button-component v-if="!isBlocked" @click="onBlockClicked" :disabled="isLoading" class="ml-0 action danger">Block client</button-component>
			<button-component v-if="isBlocked" @click="onUnBlockClicked" :disabled="isLoading" class="ml-0 action danger">Unblock client</button-component>
		</div>

		<!-- Notes -->
		<div class="client-note mb-4 pt-2 block">
			<div class="block-title">Client note</div>
			<div class="input-wrapper">
				<textarea id="clientNote" v-model="clientNote" @blur="updateClientNote"></textarea>
			</div>
		</div>

		<div class="two-cols">
			<!-- Client details -->
			<div>
				<div class="block-title">Details</div>
				<div class="client-details block">
					<div>
						<div class="client-details-row">
							<div>IP</div>
							<div>{{ options.client.ip }}</div>
						</div>
						<div class="client-details-row">
							<div>Client approved</div>
							<div>{{ (isApproved ? 'Yes' : 'No') }}</div>
						</div>
						<div class="client-details-row">
							<div>Client blocked</div>
							<div>{{ (isBlocked ? 'Yes' : 'No') }}</div>
						</div>
						<div class="client-details-row">
							<div>IP blocked</div>
							<div>
								{{ (isIpBlocked ? 'Yes' : 'No') }}
								<a v-if="!isIpBlocked" @click.prevent.stop="onBlockIpClicked" href="#">[block]</a>
								<a v-if="isIpBlocked && canUnblockIp" @click.prevent.stop="onUnBlockIpClicked" href="#">[unblock]</a>
							</div>
						</div>
						<div class="client-details-row">
							<div>UserAgent</div>
							<div><div class="truncate" @click="toggleTruncate">{{ options.client.userAgent }}</div></div>
						</div>
						<div class="client-details-row">
							<div>Access requested</div>
							<div>{{ formatDate(options.currentChallengeData.requestedAt) }}</div>
						</div>
						<div class="client-details-row" v-if="options.client.createdAtUtc">
							<div>Client created</div>
							<div>{{ formatDate(options.client.createdAtUtc) }}</div>
						</div>
						<div class="client-details-row" v-if="options.client.lastAttemptedAccessedAtUtc">
							<div>Last attempted accessed</div>
							<div>{{ formatDate(options.client.lastAttemptedAccessedAtUtc) }}</div>
						</div>
						<div class="client-details-row" v-if="options.client.lastAccessedAtUtc">
							<div>Last accessed</div>
							<div>{{ formatDate(options.client.lastAccessedAtUtc) }}</div>
						</div>
					</div>
				</div>
			</div>
			<!-- Client location -->
			<div>
				<div class="block-title">
					<div class="ipdetails-location">
						<div v-if="ipContinent" class="ipdetails-location__part">{{ ipContinent }}</div>
						<div v-if="ipFlagUrl || ipCountry" class="ipdetails-location__part">
							<img :src="ipFlagUrl" class="ipdetails-flag" />
							<span v-if="ipCountry">{{ ipCountry }}</span>
						</div>
						<div v-if="ipCity" class="ipdetails-location__part">{{ ipCity }}</div>
					</div>
				</div>
				<div class="location block" v-if="options.client.ipLocation?.success == true">
					<!-- Map -->
					<map-component class="map"
						v-if="options.client.ipLocation.latitude && options.client.ipLocation.longitude"
						:lat="options.client.ipLocation.latitude"
						:lon="options.client.ipLocation.longitude"
						:zoom="12"
						note="Client location"
						/>
				</div>
			</div>
		</div>

		<div class="challenge-statuses">
			<div class="block-title">Challenge statuses</div>
			<div class="block block--dark">
				<div class="challenges" v-if="options.allChallengeData.length > 0">
					<div v-for="challenge in options.allChallengeData" class="challenge" :title="getChallengeTooltip(challenge)">
						<div class="material-icons icon" :class="getChallengeClasses(challenge)">{{ getChallengeIcon(challenge) }}</div>
						<div>{{ getChallengeTypeIdName(challenge.type) }}</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.manual-approval-page {
    max-width: 800px;
    margin: auto;
	padding: 40px;
	@media (max-width: 800px) {
		padding: 10px;
		margin-top: 20px;
	}

	.header {
		text-align: center;
		margin-bottom: 20px;

		.title {
			font-size: 20px;
			margin: 0;
			color: var(--color--text-dark);
			div {
				font-size: 36px;
				color: var(--color--primary);
			}
		}
		.aka {
			color: var(--color--text-dark);
			font-size: 16px;
			margin-top: 10px;
			span {
				font-style: italic;
			}
		}
		.link {
			color: var(--color--text-dark);
			font-size: 16px;
		}
	}

	.easycode-wrapper {
		max-width: 600px;
		margin: auto;
		background-color: #121212;

		.easycodeTitle {
			font-size: 22px;
			text-align: center;
		}
		.easycode {
			text-align: center;
			font-size: calc(min(max(40px, 11vw),70px));
		}
	}

	.status {
		text-align: center;
		padding: 10px;
		color: var(--color--text-dark);

		&.approved {
			color: var(--color--success-base);
		}

		&.blocked {
			color: var(--color--warning-base);
		}
	}

	.actions {
		display: flex;
		margin-bottom: 20px;

		.button {
			max-width: 50%;
			flex: 1;
			height: 80px;
			font-size: min(max(18px, 4vw), 28px);
			font-weight: 600;

			&:first-child {
				margin-right: 10px;
			}
			&:last-child {
				margin-left: 10px;
			}
		}
	}

	.client-note {
		text-align: center;
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

	.two-cols {
		display: flex;
		min-height: 340px;

		> div {
			max-width: 50%;
			flex: 1;
			margin-bottom: 20px;
			display: flex;
			flex-direction: column;

			&:first-child {
				margin-right: 10px;
			}
			&:last-child {
				margin-left: 10px;
			}
		}

		.block {
			flex: 1;
		}

		@media (max-width: 600px) {
			flex-direction: column;
			> div {
				max-width: 100%;
				flex: 1;
				margin-left: 0 !important;
				margin-right: 0 !important;
			}
		}
	}

	.challenge-statuses {
		text-align: center;
		display: flex;
		justify-content: center;
		flex-direction: column;
		align-items: center;

		@media (max-width: 600px) {
			align-items: stretch;
			.challenges {
				display: flex;
				flex-direction: column;
				align-items: center;
			}
		}

		.challenge {
			margin-bottom: 5px;
			
			.icon {
				width: 24px;
				margin-right: 4px;
			}
		}
	}

	.client-details-row {
		display: flex;
		flex-wrap: nowrap;
		font-size: 14px;
		padding-bottom: 5px;
		margin-bottom: 5px;
		&:not(:last-child) {
			border-bottom: 1px solid var(--color--panel-light);
		}

		> div {
			&:first-child {
				width: 120px;
				font-weight: 600;
			}
			&:last-child {
				flex: 1;
    			max-width: 100%;
				overflow: hidden;
				font-family: monospace;
				margin-left: 10px;
			}
		}
	}

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
