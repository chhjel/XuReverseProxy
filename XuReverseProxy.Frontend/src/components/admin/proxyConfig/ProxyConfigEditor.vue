<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import DialogComponent from "@components/common/DialogComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		DialogComponent
	}
})
export default class ProxyConfigEditor extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
  	@Prop()
	value: ProxyConfig;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyConfig | null = null;

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		const changed = JSON.stringify(this.localValue) != JSON.stringify(this.value);
		if (changed) this.localValue = JSON.parse(JSON.stringify(this.value));
    }

    @Watch('localValue', { deep: true })
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.updateLocalValue();
            return;
        }

		this.$emit('update:value', this.localValue);
    }

	get sudomainPlaceholder(): string {
		return `None, using root domain: ${this.options.rootDomain}`;
	}
}
</script>

<template>
	<div class="proxyconfig-edit" v-if="localValue">
		<div>ID: <code>{{ localValue.id }}</code></div>
		<div>Enabled: <code>{{ localValue.enabled }}</code> // todo cb</div>
		<text-input-component label="Name" v-model:value="localValue.name" />
		<text-input-component label="Subdomain" v-model:value="localValue.subdomain" :placeholder="sudomainPlaceholder" />
		<text-input-component label="Port" v-model:value="localValue.port"
			type="number" :emptyIsNull="true" placeholder="Any" />
		<text-input-component label="Destination prefix" v-model:value="localValue.destinationPrefix" />

		<div class="block-title mt-4">Challenge page</div>
		<div class="block block--dark">
			<text-input-component label="Title" v-model:value="localValue.challengeTitle" />
			<div class="input-wrapper">
				<label>Description</label>
				<textarea id="challengeDescription" v-model="localValue.challengeDescription"></textarea>
			</div>
			<div>showCompletedChallenges: <code>{{ localValue.showCompletedChallenges }}</code> // todo cb</div>
			<div>showChallengesWithUnmetRequirements: <code>{{ localValue.showChallengesWithUnmetRequirements }}</code> // todo cb</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfig-edit {

}
</style>
