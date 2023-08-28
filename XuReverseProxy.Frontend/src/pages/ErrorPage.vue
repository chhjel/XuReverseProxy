<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'

interface ErrorPageFrontendModel {
	errorCode: number;
}

@Options({
	components: {}
})
export default class ErrorPage extends Vue {
  	@Prop()
	options: ErrorPageFrontendModel;

	get note(): string | null {
		if (this.options.errorCode == 404) return 'Seems this page got lost somewhere..';
		else return 'Seems something failed..';
	}
}
</script>

<template>
	<div class="error-page">
		<div class="error-page__box">
			<div class="error-page__code">{{ options.errorCode }}</div>
			<div class="error-page__note" v-if="note">{{ note }}</div>
			<a class="error-page__link" href="/">Back to root</a>
		</div>
	</div>
</template>

<style scoped lang="scss">
.error-page {
	padding-top: 20%;

	&__box {
		width: calc(max(50%, 200px));
		margin: auto;
		border: 2px solid var(--color--panel);
		padding: 20px;
		text-align: center;
	}

	&__note {
		margin-bottom: 20px;
		color: var(--color--warning-base);
		font-size: 15px;
	}

	&__code {
		font-size: 12vw;
	}
}
</style>
